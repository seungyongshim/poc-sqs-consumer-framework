namespace Service.Sqs;

public class SubscribeSqs<T> : ISubscribeSqs<T> where T : notnull
{
    public Task HandleAsync(T dto, CancellationToken ct) => throw new UnhandleDtoException(dto);
}
