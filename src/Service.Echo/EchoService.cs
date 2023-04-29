using Service.Echo.Abstractions;

namespace Service.Echo;

public class EchoService : IEchoService
{
    public EchoService(IEchoApi echoApi)
    {
        EchoApi = echoApi;
        EchoApi.Client.BaseAddress = new Uri("https://postman-echo.com/");
    }

    public IEchoApi EchoApi { get; }

    public Task<string> EchoString(string id) => EchoApi.EchoString(new EchoRequest
    {
        Id = id
    });
}
