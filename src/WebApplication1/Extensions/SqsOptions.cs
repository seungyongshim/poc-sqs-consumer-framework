global using SqsOptions = Microsoft.Extensions.Options.IOptions<System.Collections.Generic.Dictionary<WebApplication1.Extensions.AppServiceType, WebApplication1.Extensions.SqsOptionsContext>>;

namespace WebApplication1.Extensions;

public record SqsOptionsContext

{
    public IList<SqsUrlsContext> SqsUrls { get; init; } = new List<SqsUrlsContext>();

    public Func<IServiceProvider, Task<bool>> IsGreenCircuitBreakAsync { get; set; } = _ => Task.FromResult(true);
}

