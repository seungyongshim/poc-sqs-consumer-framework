namespace Service.Sqs.Config;

public record SqsOptionsContext
{
    public IList<SqsUrlsContext> SqsConfigs { get; init; } = new List<SqsUrlsContext>();

    public Func<Task<bool>> IsGreenCircuitBreakAsync { get; set; } = () => Task.FromResult(true);
}

