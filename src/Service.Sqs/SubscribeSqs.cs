using Service.Sqs.Abstractions;

namespace Service.Sqs;

public class SubscribeSqs<T> : ISubscribeSqs<T> where T : notnull
{
    public Task HandleAsync(T dto) => throw new UnhandleDtoException(dto);
}
