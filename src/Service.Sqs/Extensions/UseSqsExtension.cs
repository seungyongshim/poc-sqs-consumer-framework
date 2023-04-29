using System;
using System.Collections.Generic;
using Amazon.Extensions.NETCore.Setup;
using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Service.Abstractions;
using Service.Sqs;
using Service.Sqs.Config;
using Service.Sqs.Internal;

namespace Service.Sqs.Extensions;

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

    public static IHostBuilder UseEffect<T, TC>
    (
        this IHostBuilder host,
        T appServiceType,
        Func<TC, IServiceProvider, TC>? func = null
    ) where T : struct, Enum
      where TC : class
    {
        func ??= (o, sp) => o;

        _ = host.ConfigureServices((ctx, services) =>
        {
            TryAddDefaultAwsOptions(ctx.Configuration, services);

            _ = services.AddOptions<Dictionary<string, TC>>()
                        .BindConfiguration("")
                        .PostConfigure<IServiceProvider>((option, sp) =>
                        {
                            option[Enum.GetName(appServiceType)!] = func.Invoke(option[Enum.GetName(appServiceType)!], sp);
                        });
        });

        return host;
    }

    public static IHostBuilder UseEffectSqs<T>(this IHostBuilder host,
                                               T appServiceType,
                                               Func<SqsOptionsContext, IServiceProvider, SqsOptionsContext>? func = null) where T : struct, Enum
    {
        _ = host.UseEffect(appServiceType, func);

        _ = host.ConfigureServices((ctx, services) =>
        {
            _ = services.AddSingleton<ISqsService, SqsService>();
            _ = services.AddHttpClient("SQS");
            _ = services.AddSingleton<SqsHttpClientFactory>();
            _ = services.AddSingleton<IAmazonSQS, AmazonSQSClient>(sp => new AmazonSQSClient(new AmazonSQSConfig
            {
                RegionEndpoint = sp.GetRequiredService<AWSOptions>().Region,
                HttpClientFactory = sp.GetRequiredService<SqsHttpClientFactory>(),
            }));

            _ = services.AddTransient(typeof(ISubscribeSqs<>), typeof(SubscribeSqs<>));
            _ = services.AddHostedService(sp => new SqsHostedService<T>(sp, sp.GetRequiredService<SqsOptions>(), appServiceType));
        });

        return host;
    }
}
