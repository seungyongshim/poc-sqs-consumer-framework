using Service.Sqs.Abstractions;
using WebApplication1.Controllers;

namespace WebApplication1.SqsHandlers;

public class NewRecordSqsHandler : ISubscribeSqs<NewRecord>
{
    public int VisibleTimeOutSec { get; } = 10;

    public Task HandleAsync(NewRecord dto)
    {
        Console.WriteLine(dto);
        return Task.CompletedTask;
    }
}

public class AbstractNewRecord2Handler : ISubscribeSqs<AbstractNewRecord2>
{
    public int VisibleTimeOutSec { get; } = 10;

    public Task HandleAsync(AbstractNewRecord2 dto)
    {
        Console.WriteLine(dto);
        return Task.CompletedTask;
    }
}
