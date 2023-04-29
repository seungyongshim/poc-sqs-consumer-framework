using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Refit;
using Service.Echo.Abstractions;

namespace Service.Echo;

public static class EchoExtensions
{
    public static IHostBuilder UseEchoService(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices((context, services) =>
        {
            services.AddRefitClient<IEchoApi>()
                    .ConfigureHttpClient((sp, client) =>
                    {
                    });
            services.AddSingleton<IEchoService, EchoService>();
        });

        return hostBuilder;
    }
}
