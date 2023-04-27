namespace WebApplication1;

public class SqsOption
{
    public IList<SqsUrlsContext> SqsUrls { get; } = new List<SqsUrlsContext>();
}

public record SqsUrlsContext
{
    public required string Url { get; init; }
    public int Parallelism { get; init; } = 1;
}
