using System.ComponentModel;

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

        var tasks = option.SqsUrls.Select(x => Task.Factory.StartNew(() =>
        {
            while (true)
            {
                Thread.Sleep(1000);
                Console.WriteLine($"SqsUrl: {x}");
            }
        }, TaskCreationOptions.LongRunning));

        foreach (var task in tasks)
        {
        }

        return builder;
    }
}
