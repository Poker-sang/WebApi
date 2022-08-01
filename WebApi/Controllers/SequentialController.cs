using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;
using WebApi.TorchUtilities.Models;

namespace WebApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class SequentialController : ControllerBase
{
    private static readonly string[] _summaries =
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    [HttpPost("Find")]
    public string Post(string name) => App.Database.SequentialRecord.Find(name)?.ContentJson ?? "[]";

    [HttpPost("All")]
    public JsonArray Post()
        => new(App.Database.SequentialRecord.ToArray()
            .Select((s, index)
                => (JsonNode)new JsonObject
                {
                    ["key"] = index,
                    ["name"] = s.Name,
                    ["layers"] = s.Layers,
                    ["usedCount"] = s.UsedCount,
                    ["createdAt"] = s.CreateTime.ToUnixTimeStamp(),
                    ["remark"] = s.Remark
                }).ToArray());
}

