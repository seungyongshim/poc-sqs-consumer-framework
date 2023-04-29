using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using CommunityToolkit.HighPerformance;

namespace Service.Sqs;

internal class SqsService : ISqsService
{
    public SqsService(IAmazonSQS amazonSQS, SqsOptions option)
    {
        AmazonSQS = amazonSQS;
        Option = option;
    }

    public IAmazonSQS AmazonSQS { get; }
    public SqsOptions Option { get; }

    public async Task SendMessageAsync<T>(T AppName, object message) where T: struct, Enum
    {
        var json = TypedJsonSerializer.Serialize(message);
        var hash = json.GetDjb2HashCode();

        while (true)
        {
            try
            {
                _ = await AmazonSQS.SendMessageAsync(new SendMessageRequest
                {
                    QueueUrl = "https://sqs.ap-northeast-2.amazonaws.com/575717842801/unittest1.fifo",
                    MessageBody = json,
                    MessageGroupId = $"{hash}",
                });
                break;
            }
            catch (AmazonSQSException ex) when (ex.Message == "Request is throttled.")
            {
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
        }
    }
}

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
