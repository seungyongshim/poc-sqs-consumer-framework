using System.Net;
using System.Security.Cryptography;
using Service.Echo.Abstractions;
using static Service.Echo.Prelude;

namespace Service.Echo;

public class EchoService : IEchoService
{
    public EchoService(IEchoApi echoApi)
    {
        EchoApi = echoApi;
        EchoApi.Client.BaseAddress = new Uri("https://postman-echo.com/");
    }

    public IEchoApi EchoApi { get; }

    public async Task<string> EchoStringAsync(string id)
    {
        using var res = await Retry(() => EchoApi.EchoAsync(new EchoRequest
        {
            Id = id
        }));

        return res.Content;
    }
}
