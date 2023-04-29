namespace Service.Echo.Abstractions
{
    public interface IEchoService
    {
        Task<string> EchoString(string id);
    }
}