using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;
using WebApi.TorchUtilities.Models;

namespace WebApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] _summaries =
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    [HttpPost]
    public Net Post() => new MobileNet();
    [HttpGet]
    public JsonArray Get() => new()
        {
            new JsonObject
            {
                ["key"] = 1,
                ["name"] = "MobileNet",
                ["layers"] = 20,
                ["usedCount"] = 2,
                ["createdAt"] = Now,
                ["remark"] = "神经网络"
            },
            new JsonObject
            {
                ["key"] = 2,
                ["name"] = "ConvDw",
                ["layers"] = 6,
                ["usedCount"] = 20,
                ["createdAt"] = Now,
                ["remark"] = "深度卷积网络"
            },


        };

    private readonly DateTime _start = new(1970, 1, 1, 8, 0, 0);
    private long Now => (long)(DateTime.Now - _start).TotalMilliseconds;
}

