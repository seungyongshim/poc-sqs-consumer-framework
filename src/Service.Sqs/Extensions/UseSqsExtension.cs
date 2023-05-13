using System;
using System.Collections.Generic;
using Amazon.Extensions.NETCore.Setup;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Service.Abstractions;
using Service.Sqs.Abstractions;
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
        T appName,
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
                            option[Enum.GetName(appName)!] = func.Invoke(option[Enum.GetName(appName)!], sp);
                        });
        });

        return host;
    }

    public static IHostBuilder UseEffectSqs<T>
    (
        this IHostBuilder host,
        T appName,
        Func<SqsOptionsContext, IServiceProvider, SqsOptionsContext>? func = null
    ) where T : struct, Enum
    {
        _ = host.UseEffect(appName, func);
        _ = host.ConfigureServices((ctx, services) =>
        {
            _ = services.AddSingleton<ISqsService, SqsService>();
            _ = services.AddHttpClient("SqsProducer").ConfigureHttpMessageHandlerBuilder(config =>
            {
                config.PrimaryHandler = new HttpClientHandler
                {
                    MaxConnectionsPerServer = 2,
                };
            });

            _ = services.AddSingleton<IAmazonSQSProducer, AmazonSQSProducer>(sp => new(config: new()
            {
                RegionEndpoint = sp.GetRequiredService<AWSOptions>().Region,
                HttpClientFactory = new SqsHttpClientFactory(sp.GetRequiredService<IHttpClientFactory>(), "SqsProducer"),
            }));

            _ = services.AddTransient<PolicyHttpMessageHandler>();

            _ = services.AddHttpClient("SqsConsumer")
                        //.AddHttpMessageHandler<PolicyHttpMessageHandler>()
                        .ConfigurePrimaryHttpMessageHandler(sp => new HttpClientHandler
                        {
                            MaxConnectionsPerServer = 2,
                        });
                        
                        

            _ = services.AddSingleton<IAmazonSQSConsumer, AmazonSQSConsumer>(sp => new(config: new()
            {
                RegionEndpoint = sp.GetRequiredService<AWSOptions>().Region,
                HttpClientFactory = new SqsHttpClientFactory(sp.GetRequiredService<IHttpClientFactory>(), "SqsConsumer"),
            }));

            _ = services.AddTransient(typeof(ISubscribeSqs<>), typeof(SubscribeSqs<>));
            _ = services.AddHostedServices(sp => new SqsHostedService<T>(sp, appName));
        });

        return host;
    }
}

public interface IAmazonSQSConsumer
{
    Task<DeleteMessageResponse> DeleteMessageAsync(DeleteMessageRequest request, CancellationToken cancellationToken = default);
    Task<ReceiveMessageResponse> ReceiveMessageAsync(ReceiveMessageRequest request, CancellationToken cancellationToken = default);
}

public class AmazonSQSConsumer : AmazonSQSClient, IAmazonSQSConsumer
{
    public AmazonSQSConsumer(AmazonSQSConfig config) : base(config)
    {
    }
}

public interface IAmazonSQSProducer
{
    Task<SendMessageResponse> SendMessageAsync(SendMessageRequest request, CancellationToken cancellationToken = default);
}

public class AmazonSQSProducer : AmazonSQSClient, IAmazonSQSProducer
{
   public AmazonSQSProducer(AmazonSQSConfig config) : base(config)
   {
   }
}
