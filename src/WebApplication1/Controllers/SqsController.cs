using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class SqsController : ControllerBase
{
    public SqsController(IAmazonSQS amazonSQS) => AmazonSQS = amazonSQS;

    public IAmazonSQS AmazonSQS { get; }

    [HttpGet(Name = "Testbed")]
    public async Task Get()
    {
        var guid = Guid.NewGuid().ToString();   

        var q = from x in Enumerable.Range(0, 10)
                from y in Enumerable.Range(0, 2)
                select Task.Run(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            _ = await AmazonSQS.SendMessageAsync(new SendMessageRequest
                            {
                                QueueUrl = "https://sqs.ap-northeast-2.amazonaws.com/575717842801/unittest1.fifo",
                                MessageBody = $"{x}",
                                MessageGroupId = $"{y}{guid}",
                            });
                            break;
                        }
                        catch (AmazonSQSException ex) when (ex.Message == "Request is throttled.")
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(10));
                            Console.WriteLine("Catch.. Request is throttled");
                        }
                    }
                });

        foreach (var item in q)
        {
            await item;
        }
    }
}



