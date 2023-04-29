using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.SQS;
using Microsoft.Extensions.DependencyInjection;
using WebApplication1.Controllers;

namespace WebApplication1.Extensions;

public static partial class UseSqsExtension
{
    private static void TryAddDefaultAwsOptions(IConfiguration config, IServiceCollection services)
    {
        if (services.Any(x => x.ServiceType == typeof(AWSOptions)))
        {
            return;
        }

        _ = services.AddDefaultAWSOptions(config.GetAWSOptions());
    }

    public static IHostBuilder UseEffect<T>
    (
        this IHostBuilder host,
        AppServiceType appServiceType,
        Func<T, IServiceProvider, T>? func = null
    ) where T : class
    {
        func ??= (o, sp) => o;

        _ = host.ConfigureServices((ctx, services) =>
        {
            TryAddDefaultAwsOptions(ctx.Configuration, services);

            _ = services.AddOptions<Dictionary<AppServiceType, T>>()
                        .BindConfiguration("")
                        .PostConfigure<IServiceProvider>((option, sp) =>
                        {
                            option[appServiceType] = func.Invoke(option[appServiceType], sp);
                        });
        });

        return host;
    }

    public static IHostBuilder UseEffectSqs(this IHostBuilder host,
                                            AppServiceType appServiceType,
                                            Func<SqsOptionsContext, IServiceProvider, SqsOptionsContext>? func = null)
    {
        _ = host.UseEffect(appServiceType, func);

        _ = host.ConfigureServices((ctx, services) =>
        {
            _ = services.AddHttpClient("SQS");
            _ = services.AddSingleton<SqsHttpClientFactory>();
            _ = services.AddSingleton<IAmazonSQS, AmazonSQSClient>(sp => new AmazonSQSClient(new AmazonSQSConfig
            {
                RegionEndpoint = sp.GetRequiredService<AWSOptions>().Region,
                HttpClientFactory = sp.GetRequiredService<SqsHttpClientFactory>(),
                //CacheHttpClient = false,
                //BufferSize = 4096*2,
                //HttpClientCacheSize = 4096,
                //MaxErrorRetry = 1,
            }));
            
            _ = services.AddTransient(typeof(ISubscribeSqs<>), typeof(SubscribeSqs<>));
            _ = services.AddHostedService(sp => new SqsHostedService(sp, sp.GetRequiredService<SqsOptions>().Value[appServiceType]));
        });

        return host;
    }
}

public class SqsHttpClientFactory : HttpClientFactory
{
    public IHttpClientFactory HttpClientFactory { get; }

    public SqsHttpClientFactory(IHttpClientFactory httpClientFactory)
    {
        HttpClientFactory = httpClientFactory;
    }

    public override HttpClient CreateHttpClient(IClientConfig clientConfig)
    {
        return HttpClientFactory.CreateClient("SQS");
    }
    public override bool UseSDKHttpClientCaching(IClientConfig clientConfig)
    {
        // return false to indicate that the SDK should not cache clients internally
        return false;
    }
    public override bool DisposeHttpClientsAfterUse(IClientConfig clientConfig)
    {
        // return false to indicate that the SDK shouldn't dispose httpClients because they're cached in your pool
        return false;
    }
    public override string GetConfigUniqueString(IClientConfig clientConfig)
    {
        // has no effect because UseSDKHttpClientCaching returns false
        return null;
    }
}
