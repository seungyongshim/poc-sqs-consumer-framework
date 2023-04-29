using Amazon.Runtime;

namespace Service.Sqs.Internal;

internal class SqsHttpClientFactory : HttpClientFactory
{
    public IHttpClientFactory HttpClientFactory { get; }

    public SqsHttpClientFactory(IHttpClientFactory httpClientFactory) => HttpClientFactory = httpClientFactory;
    public override HttpClient CreateHttpClient(IClientConfig clientConfig) => HttpClientFactory.CreateClient("SQS");
    public override bool UseSDKHttpClientCaching(IClientConfig clientConfig) => false;
    public override bool DisposeHttpClientsAfterUse(IClientConfig clientConfig) => false;
    public override string? GetConfigUniqueString(IClientConfig clientConfig) => null;
}
