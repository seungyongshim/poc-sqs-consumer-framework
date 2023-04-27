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
                    select Task.Factory.StartNew(() =>
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var msg = new HelloDto() { Name = "Hello" };
                            var t = typeof(ISubscribeSqs<>).MakeGenericType(msg.GetType());

                            var c = ServiceProvider.GetRequiredService(t);
                            var m = t.GetMethod("Handle");

                            m.Invoke(c, new object[] { msg, cancellationToken });
                            Thread.Sleep(1000);
                        }
                    }, TaskCreationOptions.LongRunning);

        foreach (var task in tasks)
        {
        }

        return Task.CompletedTask;
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;


}
