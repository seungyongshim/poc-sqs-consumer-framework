using Refit;

namespace Service.Echo;

public interface IEchoApi
{
    HttpClient Client { get; }

    [Post("/post")]
    Task<IApiResponse<string>> EchoAsync([Body] EchoRequest id);
}
