using Service.Sqs.Abstractions;
using WebApplication1.Controllers;

public class NewRecordSqsHandler : ISubscribeSqs<NewRecord>
{
    public Task HandleAsync(NewRecord dto)
    {
        Console.WriteLine(dto);
        return Task.CompletedTask;
    }
}
