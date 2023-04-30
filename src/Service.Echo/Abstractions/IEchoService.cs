namespace Service.Echo.Abstractions;

public interface IEchoService
{
    Task<string> EchoStringAsync(string id);
}
