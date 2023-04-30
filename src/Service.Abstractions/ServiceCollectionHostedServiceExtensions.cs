using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Service.Abstractions;

public static class ServiceCollectionHostedServiceExtensions
{
    public static IServiceCollection AddHostedServices<THostedService>(this IServiceCollection services)
        where THostedService : class, IHostedService
    {
        services.Add(ServiceDescriptor.Singleton<IHostedService, THostedService>());

        return services;
    }

    public static IServiceCollection AddHostedServices<THostedService>(this IServiceCollection services, Func<IServiceProvider, THostedService> implementationFactory)
        where THostedService : class, IHostedService
    {
        services.Add(ServiceDescriptor.Singleton<IHostedService>(implementationFactory));

        return services;
    }
}
