using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.Mvc;
using Service.Abstractions;
using Service.Sqs;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class SqsController : ControllerBase
{
    public SqsController(ISqsService sqsService) => SqsService = sqsService;

    public ISqsService SqsService { get; }

    [HttpGet(Name = "Testbed")]
    public async Task Get()
    {
        var guid = Guid.NewGuid().ToString();   

        var q = from x in Enumerable.Range(0, 10)
                from y in Enumerable.Range(0, 2)
                select SqsService.SendMessageAsync(AppNameType.ServiceA, new NewRecord(x,y, guid));

        foreach (var item in q)
        {
            await item;
        }
    }
}

public record NewRecord(int Name, int Value, string guid);