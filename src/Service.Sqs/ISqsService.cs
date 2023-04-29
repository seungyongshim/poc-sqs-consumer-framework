namespace Service.Sqs;

public interface ISqsService
{
    Task SendMessageAsync<T>(T AppName, object message) where T : struct, Enum;
}
