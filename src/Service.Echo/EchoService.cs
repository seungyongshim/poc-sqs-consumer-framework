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

    public async Task<string> EchoString(string id)
    {
        if (RandomNumberGenerator.GetInt32(3) == 0)
        {
            throw new Exception("Random error");
        }

        using var res = await Retry(() => EchoApi.EchoAsync(new EchoRequest
        {
            Id = id
        }));

        return res.Content;
    }
}
