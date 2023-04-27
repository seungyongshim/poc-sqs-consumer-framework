using WebApplication1.Dto;

namespace WebApplication1.Controllers;

public interface ISubscribeSqs<T>
{
    Task Handle(T dto, CancellationToken ct);
}

public class SubscribeSqs<T> : ISubscribeSqs<T>
{
    public Task Handle(T dto, CancellationToken ct) => dto switch
    {
        HelloDto => Task.Run(() => Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}]Handle: {dto}")),
        _ => Task.CompletedTask
    };
}
