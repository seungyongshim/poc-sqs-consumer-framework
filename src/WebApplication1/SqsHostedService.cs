using WebApplication1.Controllers;
using WebApplication1.Dto;

namespace WebApplication1;

public class SqsHostedService : IHostedService
{
    public SqsHostedService(IServiceProvider sp,  SqsOption option)
    {
        ServiceProvider  = sp;
        Option = option;
    }

    public IServiceProvider ServiceProvider  { get; }
    public SqsOption Option { get; }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var tasks = from x in Option.SqsUrls
                    from y in Enumerable.Range(0, x.Parallelism)
                    select Task.Factory.StartNew(async () =>
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            await using var scope = ServiceProvider.CreateAsyncScope();
                            var msg = new HelloDto() { Name = $"{y}" };
                            var t = typeof(ISubscribeSqs<>).MakeGenericType(msg.GetType());

                            var c = scope.ServiceProvider.GetRequiredService(t);
                            var m = t.GetMethod("Handle");

                            m.Invoke(c, new object[] { msg, cancellationToken });
                            await Task.Delay(1000).ConfigureAwait(false);
                        }
                    }, TaskCreationOptions.LongRunning);

        foreach (var task in tasks)
        {
        }

        return Task.CompletedTask;
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;


}
