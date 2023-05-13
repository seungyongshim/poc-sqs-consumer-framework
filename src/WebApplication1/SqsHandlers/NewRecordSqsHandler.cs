using Service.Sqs.Abstractions;
using WebApplication1.Controllers;

namespace WebApplication1.SqsHandlers;

public class AbstractNewRecordHandler : ISubscribeSqs<AbstractNewRecord>
{
    public int VisibleTimeOutSec { get; } = 10;

    public Task HandleAsync(AbstractNewRecord dto)
    {
     //   throw new NotImplementedException();
       // Console.WriteLine(dto);
        return Task.CompletedTask;
    }
}
