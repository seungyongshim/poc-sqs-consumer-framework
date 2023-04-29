using Service.Sqs;

public class StringSqsHandler : ISubscribeSqs<string>
{
    public Task HandleAsync(string dto, CancellationToken ct)
    {
        Console.WriteLine(dto);
        return Task.CompletedTask;
    }
}
