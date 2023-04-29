using Microsoft.AspNetCore.Mvc;
using Service.Abstractions;
using Service.Echo.Abstractions;
using Service.Sqs.Abstractions;
using System.Xml.Linq;

namespace WebApplication1.Controllers;


[ApiController]
[Route("[controller]")]
public class EchoController : ControllerBase
{
    public EchoController(IEchoService sqsService) => SqsService = sqsService;

    public IEchoService SqsService { get; }

    [HttpPost]
    public Task<string> Get(string data)
    {
        return SqsService.EchoString(data);
    }
}
