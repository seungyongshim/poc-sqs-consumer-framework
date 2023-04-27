using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using WebApplication1.Controllers;

namespace WebApplication1;

public static class UseSqsExtension
{
    public static IHostBuilder UseSqs(this IHostBuilder builder, Action<SqsOption> action)
    {
        var option = new SqsOption()
        {

        };
        // Controller를 모두 확인해서 SubscribeSqs를 찾음
        action.Invoke(option);

        builder.ConfigureServices(services =>
        {
            services.AddTransient(typeof(ISubscribeSqs<>), typeof(SubscribeSqs<>));
            services.AddHostedService<SqsHostedService>(sp => new SqsHostedService(sp, option));
        });

        
        return builder;
    }
}
