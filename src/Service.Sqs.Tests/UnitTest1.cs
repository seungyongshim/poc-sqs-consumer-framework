using Service.Sqs;

public abstract class AbstractNewRecord
{
    public string Name { get; init; } = string.Empty;
}

public class NewRecord : AbstractNewRecord
{
    public int Value { get; init; }
}

public class Test
{
    [Fact]
    public void Test1()
    {
        var json = TypedJsonSerializer.Serialize(new NewRecord
        {
            Name = "Test",
            Value = 1
        });

        var dec = TypedJsonSerializer.Deserialize(json);


    }
}
