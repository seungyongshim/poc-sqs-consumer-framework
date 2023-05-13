using Amazon.Runtime;

namespace Service.Sqs.Internal;

internal class SqsHttpClientFactory : HttpClientFactory
{
    public IHttpClientFactory HttpClientFactory { get; }
    public string Name { get; }

    public SqsHttpClientFactory(IHttpClientFactory httpClientFactory, string name)
    {
        HttpClientFactory = httpClientFactory;
        Name = name;
    }

    public override HttpClient CreateHttpClient(IClientConfig clientConfig) => HttpClientFactory.CreateClient(Name);
    public override bool UseSDKHttpClientCaching(IClientConfig clientConfig) => false;
    public override bool DisposeHttpClientsAfterUse(IClientConfig clientConfig) => false;
    public override string? GetConfigUniqueString(IClientConfig clientConfig) => null;
}
