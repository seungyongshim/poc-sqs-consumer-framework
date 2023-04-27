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

    [SubscribeSqs<HelloDto>()]
    public Task DoWork(HelloDto dto) => default;
}


