using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Service.Sqs.Abstractions;
using Service.Sqs.Config;
using Service.Sqs.Internal;

namespace Service.Sqs;

//https://github.com/awslabs/aws-dotnet-messaging/blob/main/src/AWS.Messaging/SQS/SQSMessagePoller.cs#L18

public class SqsHostedService<T> : IHostedService where T : struct, Enum
{
    public SqsHostedService(IServiceProvider sp, T index)
    {
        ServiceProvider = sp;
        Option = sp.GetRequiredService<SqsOptions>().Value[Enum.GetName(index)!];
        Logger = sp.GetRequiredService<ILogger<SqsHostedService<T>>>();
    }

    public IServiceProvider ServiceProvider { get; }
    public SqsOptionsContext Option { get; }
    public ILogger Logger {get;}

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var sqs = ServiceProvider.GetRequiredService<IAmazonSQS>();
        var single = new SingleThreadTaskScheduler();

        var tasks =
            from config in Option.SqsConfigs
            from y in Enumerable.Range(0, config.Parallelism)
            select Task.Factory.StartNew(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (await Option.IsGreenCircuitBreakAsync() is false)
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

                        var q =
                            from a in res.Messages.Select((x, i) => (x, i))
                            select Task.Factory.StartNew(async () =>
                            {
                                //if (a.i % 2 == 1) throw new Exception("Test");


                                var msg = TypedJsonSerializer.Deserialize(a.x.Body);
                                var type = typeof(ISubscribeSqs<>).MakeGenericType(msg.GetType());
                                var c = scope.ServiceProvider.GetRequiredService(type);
                                var m = type.GetMethod("HandleAsync");
                                var t1 = m?.Invoke(c, new object[] { msg }) switch
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
                               
                            }, default, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();

                        foreach(var x in q)
                        {
                            await x;
                        }
                    }
                    catch (Exception ex)
                    {
                        await Task.Delay(1000);
                        Logger.LogError(ex, "");
                    }
                }
            }, default, TaskCreationOptions.LongRunning, single).Unwrap();

        _ = Task.WhenAll(tasks);

        return Task.CompletedTask;
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;


}
