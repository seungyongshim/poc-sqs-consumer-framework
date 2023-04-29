namespace Service.Echo;

public record EchoRequest
{
    public required string Id { get; init; }
}
