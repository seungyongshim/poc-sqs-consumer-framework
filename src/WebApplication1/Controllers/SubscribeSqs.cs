using WebApplication1.Dto;

namespace WebApplication1.Controllers;

public class SubscribeSqs<T> : ISubscribeSqs<T>
{
    public Task HandleAsync(T dto, CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}
