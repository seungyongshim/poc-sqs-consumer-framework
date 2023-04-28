using WebApplication1.Controllers;
using WebApplication1.Dto;
using WebApplication1.Extensions;

namespace WebApplication1;

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
                                var msg = new HelloDto() { Name = $"{y}" };
                                var type = typeof(ISubscribeSqs<>).MakeGenericType(msg.GetType());

                                var c = scope.ServiceProvider.GetRequiredService(type);
                                var m = type.GetMethod("Handle");

                                var t1 = m?.Invoke(c, new object[] { msg, cancellationToken }) switch
                                {
                                    Task v => v,
                                    _ => Task.CompletedTask
                                };

                                await t1;
                            }
                            catch
                            {
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
