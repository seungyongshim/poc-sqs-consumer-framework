using Refit;

namespace Service.Echo;

public interface IEchoApi
{
    HttpClient Client { get; }

    [Post("/post")]
    Task<string> EchoString([Body] EchoRequest id);
}
