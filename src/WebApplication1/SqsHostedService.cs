using Amazon.SQS;
using WebApplication1.Controllers;
using WebApplication1.Dto;
using WebApplication1.Extensions;

namespace WebApplication1;

//https://github.com/awslabs/aws-dotnet-messaging/blob/main/src/AWS.Messaging/SQS/SQSMessagePoller.cs#L18

public class SqsHostedService : IHostedService
{
    public SqsHostedService(IServiceProvider sp, SqsOptionsContext option)
    {
        ServiceProvider = sp;
        Option = option;
    }

    public IServiceProvider ServiceProvider { get; }
    public SqsOptionsContext Option { get; }

    public Task StartAsync(CancellationToken cancellationToken)
    {

        var single = new SingleThreadTaskScheduler();

        var tasks = from x in Option.SqsUrls
                    from y in Enumerable.Range(0, x.Parallelism)
                    select Task.Factory.StartNew(async () =>
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            if ((await Option.IsGreenCircuitBreakAsync(ServiceProvider)) is false)
                            {
                                await Task.Delay(TimeSpan.FromSeconds(1));
                                continue; // circuit break
                            }

                            try
                            {
                                await using var scope = ServiceProvider.CreateAsyncScope();
                                var sqs = scope.ServiceProvider.GetRequiredService<IAmazonSQS>();

                                var res = await sqs.ReceiveMessageAsync(new Amazon.SQS.Model.ReceiveMessageRequest
                                {
                                    QueueUrl = x.Url,
                                    MaxNumberOfMessages = 10,
                                }, cancellationToken);

                                var msg = new HelloDto() { Name = $"{y}" };
                                var type = typeof(ISubscribeSqs<>).MakeGenericType(msg.GetType());

                                var c = scope.ServiceProvider.GetRequiredService(type);
                                var m = type.GetMethod("HandleAsync");

                                var t1 = m?.Invoke(c, new object[] { msg, cancellationToken }) switch
                                {
                                    Task v => v,
                                    _ => Task.CompletedTask
                                };

                                await t1;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        }
                    }, default, TaskCreationOptions.LongRunning, single);

        foreach (var task in tasks)
        {
        }

        return Task.CompletedTask;
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;


}
