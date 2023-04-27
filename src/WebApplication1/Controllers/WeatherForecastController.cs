using Microsoft.AspNetCore.Mvc;
using WebApplication1.Dto;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<ActionResult> Get()
    {

        return Ok();
    }
}

public class HelloDtoHandler : ISubscribeSqs<HelloDto>
{
    public Task Handle(HelloDto dto, CancellationToken ct) 
    {
            Console.WriteLine($"SqsUrl: {dto}");
        return Task.CompletedTask;
    }
}




