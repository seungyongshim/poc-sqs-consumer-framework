namespace WebApplication1.Extensions;

public record SqsUrlsContext
{
    public required string Url { get; init; }
    public int Parallelism { get; init; } = 1;
}

