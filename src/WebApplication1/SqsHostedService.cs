using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
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
        var sqs = ServiceProvider.GetRequiredService<IAmazonSQS>();

        //var sqs = new AmazonSQSClient(new AmazonSQSConfig
        //{
            
        //    MaxConnectionsPerServer = 1
        //});

        var single = new SingleThreadTaskScheduler();

        var tasks = from config in Option.SqsConfigs
                    from y in Enumerable.Range(0, config.Parallelism)
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
                                

                                var res = await sqs.ReceiveMessageAsync(new ReceiveMessageRequest
                                {
                                    QueueUrl = config.Url,
                                    MaxNumberOfMessages = config.MaxNumberOfMessages,
                                    WaitTimeSeconds = 20,
                                    //VisibilityTimeout = _configuration.VisibilityTimeout,
                                    //WaitTimeSeconds = _configuration.WaitTimeSeconds,
                                    AttributeNames = new List<string> { "All" },
                                    MessageAttributeNames = new List<string> { "All" }
                                }, cancellationToken);

                                var q = from a in res.Messages.Select((x, i) => (x, i))
                                        select Task.Factory.StartNew(async () =>
                                        {
                                            try
                                            {
                                                //var msg = JsonSerializer.Deserialize<HelloDto>(a.Body);

                                                if (a.i % 2 == 1)
                                                    throw new Exception();

                                                var msg = new HelloDto() { Name = $"{y} {a.i} {a.x.Body}" };
                                                var type = typeof(ISubscribeSqs<>).MakeGenericType(msg.GetType());
                                                var c = scope.ServiceProvider.GetRequiredService(type);
                                                var m = type.GetMethod("HandleAsync");
                                                var t1 = m?.Invoke(c, new object[] { msg, cancellationToken }) switch
                                                {
                                                    Task v => v,
                                                    _ => Task.CompletedTask
                                                };

                                                Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}]Handle: {msg}, {a.x.Attributes["MessageGroupId"]}");

                                                await t1;
                                                await sqs.DeleteMessageAsync(new DeleteMessageRequest
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
