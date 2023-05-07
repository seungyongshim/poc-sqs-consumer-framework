using Microsoft.AspNetCore.Mvc;
using Service.Abstractions;
using Service.Sqs.Abstractions;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class SqsController : ControllerBase
{
    public SqsController(ISqsService sqsService) => SqsService = sqsService;

    public ISqsService SqsService { get; }

    [HttpGet]
    public async Task Get()
    {
        var guid = Guid.NewGuid().ToString();

        var q = from x in Enumerable.Range(0, 100)
                from y in Enumerable.Range(0, 2)
                select SqsService.SendMessageAsync(y switch
                {
                    0 => AppNameType.App1,
                    1 => AppNameType.App2
                }, new NewRecord2(y, guid)
                {
                    Name = x.ToString(),
                });

        foreach (var item in q)
        {
            await item;
        }
    }
}

public record NewRecord(int Value, string Guid): AbstractNewRecord;
public record NewRecord2(int Value, string Guid): AbstractNewRecord;

public abstract record AbstractNewRecord
{
    public string Name { get; init; } = string.Empty;
}
