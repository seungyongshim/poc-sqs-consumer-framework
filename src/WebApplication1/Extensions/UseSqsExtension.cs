using Amazon.Extensions.NETCore.Setup;
using Amazon.SQS;
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
            _ = services.AddAWSService<IAmazonSQS>();
            _ = services.AddTransient(typeof(ISubscribeSqs<>), typeof(SubscribeSqs<>));
            _ = services.AddHostedService(sp => new SqsHostedService(sp, sp.GetRequiredService<SqsOptions>().Value[appServiceType]));
        });

        return host;
    }


    
}
