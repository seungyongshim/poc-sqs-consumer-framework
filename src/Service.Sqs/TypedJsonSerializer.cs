using System.Reflection;
using System.Text.Json;

namespace Service.Sqs;

public static class TypedJsonSerializer
{
    public static JsonSerializerOptions JsonSerializerOptions { get; } = new JsonSerializerOptions
    {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        IgnoreReadOnlyProperties = true,
        IgnoreReadOnlyFields = true,
    };

    public static JsonDocumentOptions JsonDocumentOptions { get; } = new JsonDocumentOptions
    {
       AllowTrailingCommas = true,
       CommentHandling = JsonCommentHandling.Skip
    };


    public static string Serialize<T>(T obj) where T : notnull
    {
        var json = JsonSerializer.SerializeToNode(obj, JsonSerializerOptions)!;

        json["_an"] = obj.GetType().Assembly.ToString();
        json["_tn"] = obj.GetType().FullName;

        return json.ToJsonString();
    }
    public static object? Deserialize(string json)
    {
        var q = JsonDocument.Parse(json, JsonDocumentOptions);

        var assemblyName = q.RootElement.GetProperty("_an").GetString()!;
        var typeName = q.RootElement.GetProperty("_tn").GetString()!;

        var @type = Assembly.Load(assemblyName).GetType(typeName)!;

        return JsonSerializer.Deserialize(json, @type, JsonSerializerOptions);
    }
}
