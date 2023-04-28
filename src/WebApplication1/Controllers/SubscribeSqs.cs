using WebApplication1.Dto;

namespace WebApplication1.Controllers;

public class SubscribeSqs<T> : ISubscribeSqs<T>
{
    public async Task HandleAsync(T dto, CancellationToken ct)
    {
        await Task.Delay(1000);
        Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}]Handle: {dto}");
    }
}
