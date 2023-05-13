using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using CommunityToolkit.HighPerformance;
using Service.Sqs.Abstractions;
using Service.Sqs.Extensions;

namespace Service.Sqs;

internal class SqsService : ISqsService
{
    public SqsService(IAmazonSQSProducer amazonSQS, SqsOptions option)
    {
        AmazonSQS = amazonSQS;
        Option = option;
    }

    public IAmazonSQSProducer AmazonSQS { get; }
    public SqsOptions Option { get; }

    public async Task SendMessageAsync<T>(T AppName, object message) where T: struct, Enum
    {
        var json = TypedJsonSerializer.Serialize(message);
        var hash = Math.Abs(json.GetDjb2HashCode());
        var urls = Option.Value[Enum.GetName(AppName)!].SqsConfigs.Select(x => x.Url).ToArray();

        //while (true)
        {
            //try
            //{
                _ = await AmazonSQS.SendMessageAsync(new SendMessageRequest
                {
                    QueueUrl = urls[hash % urls.Length],
                    MessageBody = json,
                    MessageGroupId = $"{hash}",
                });

                return;
            //}
            //catch (AmazonSQSException ex) when (ex.Message == "Request is throttled.")
            //{
            //    await Task.Delay(TimeSpan.FromMilliseconds(10));
            //}
        }
    }
}
