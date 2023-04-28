using Microsoft.AspNetCore.Mvc;
using WebApplication1.Dto;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    [HttpGet(Name = "GetWeatherForecast")]
    public ActionResult Get()
    {

        return Ok();
    }
}



