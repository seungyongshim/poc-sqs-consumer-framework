using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Service.Sqs.Config;
using Service.Sqs.Internal;

namespace Service.Sqs;

//https://github.com/awslabs/aws-dotnet-messaging/blob/main/src/AWS.Messaging/SQS/SQSMessagePoller.cs#L18

public class SqsHostedService<T> : IHostedService where T : struct, Enum
{
    public SqsHostedService(IServiceProvider sp, SqsOptions option, T index)
    {
        ServiceProvider = sp;
        Option = option.Value[Enum.GetName(index)!];
    }

    public IServiceProvider ServiceProvider { get; }
    public SqsOptionsContext Option { get; }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var sqs = ServiceProvider.GetRequiredService<IAmazonSQS>();
        var single = new SingleThreadTaskScheduler();

        var tasks = from config in Option.SqsConfigs
                    from y in Enumerable.Range(0, config.Parallelism)
                    select Task.Factory.StartNew(async () =>
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            if (await Option.IsGreenCircuitBreakAsync(ServiceProvider) is false)
                            {
                                await Task.Delay(TimeSpan.FromSeconds(1));
                                continue; // circuit break
                            }

                            try
                            {
                                await using var scope = ServiceProvider.CreateAsyncScope();
                                var res = await sqs.ReceiveMessageAsync(new ReceiveMessageRequest
                                {
                                    QueueUrl = config.Url,
                                    MaxNumberOfMessages = config.MaxNumberOfMessages,
                                    WaitTimeSeconds = 20,
                                    AttributeNames = new List<string> { "All" },
                                    MessageAttributeNames = new List<string> { "All" }
                                }, cancellationToken);

                                var q = from a in res.Messages.Select((x, i) => (x, i))
                                        select Task.Factory.StartNew(async () =>
                                        {
                                            try
                                            {
                                                //var msg = JsonSerializer.Deserialize<HelloDto>(a.Body);

                                                //if (a.i % 2 == 1)
                                                //    throw new Exception();

                                                var msg = $"{y} {a.i} {a.x.Body}";
                                                var type = typeof(ISubscribeSqs<>).MakeGenericType(msg.GetType());
                                                var c = scope.ServiceProvider.GetRequiredService(type);
                                                var m = type.GetMethod("HandleAsync");
                                                var t1 = m?.Invoke(c, new object[] { msg, cancellationToken }) switch
                                                {
                                                    Task v => v,
                                                    _ => Task.CompletedTask
                                                };

                                                await t1;
                                                _ = await sqs.DeleteMessageAsync(new DeleteMessageRequest
                                                {
                                                    QueueUrl = config.Url,
                                                    ReceiptHandle = a.x.ReceiptHandle
                                                }, cancellationToken);
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine(ex);
                                            }
                                        }, default, TaskCreationOptions.LongRunning, single).Unwrap();

                                await Task.WhenAll(q);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        }
                    }, default, TaskCreationOptions.LongRunning, single).Unwrap();

        foreach (var task in tasks)
        {
        }

        return Task.CompletedTask;
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;


}
