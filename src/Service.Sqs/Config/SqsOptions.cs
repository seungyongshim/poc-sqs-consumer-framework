namespace Service.Sqs.Config;

public record SqsOptionsContext
{
    public IList<SqsUrlsContext> SqsConfigs { get; init; } = new List<SqsUrlsContext>();

    public Func<IServiceProvider, Task<bool>> IsGreenCircuitBreakAsync { get; set; } = _ => Task.FromResult(true);
}

