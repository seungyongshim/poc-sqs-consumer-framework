namespace WebApplication1.Controllers;

public interface ISubscribeSqs<T>
{
    Task HandleAsync(T dto, CancellationToken ct);
}
