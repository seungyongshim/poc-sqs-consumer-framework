using Microsoft.AspNetCore.Mvc;
using Service.Echo.Abstractions;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class EchoController : ControllerBase
{
    [HttpPost]
    public async Task Post
    (
        [FromServices] IEchoService echoService,
        string data
    )
    {
        var ret = await Task.Run(async () =>
        {
            try
            {
                return Results.Ok(new
                {
                    Echo = await echoService.EchoStringAsync(data)
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: $"{ex.GetType().Name}: {ex.Message}");
            }
        });

        await ret.ExecuteAsync(HttpContext);
    }
}
