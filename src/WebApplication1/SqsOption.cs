namespace WebApplication1;

public class SqsOption
{
    public IList<Type> Subscribes { get; } = new List<Type>();
    public IList<string> SqsUrls { get; } = new List<string>();
}
