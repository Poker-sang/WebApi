using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Nodes;
using WebApi.Database;
using WebApi.TorchUtilities.Layers;
using WebApi.TorchUtilities.Services;

namespace WebApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class SequentialController : ControllerBase
{
    [HttpPost("All")]
    public JsonArray All()
        => new(App.Database.SequentialRecord.ToArray()
            .Select((s, index)
                => (JsonNode)new JsonObject
                {
                    ["key"] = index,
                    [s.Name.CamelName()] = s.Name,
                    [s.Layers.CamelName()] = s.Layers,
                    [s.UsedCount.CamelName()] = s.UsedCount,
                    [s.CreateTime.CamelName()] = s.CreateTime.ToUnixTimeStamp(),
                    [s.Remark.CamelName()] = s.Remark
                }).ToArray());

    [HttpPost("Find")]
    public JsonObject? Find(string name) =>
        App.Database.SequentialRecord.Find(name) is not { } s
            ? null
            : new JsonObject
            {
                [s.Name.CamelName()] = s.Name,
                [s.Layers.CamelName()] = s.Layers,
                [s.UsedCount.CamelName()] = s.UsedCount,
                [s.CreateTime.CamelName()] = s.CreateTime.ToUnixTimeStamp(),
                [s.Remark.CamelName()] = s.Remark
            };

    [HttpPost("Find/Property")]
    public dynamic? FindProperty(string sequentialName, string propertyName) =>
        App.Database.SequentialRecord.Find(sequentialName) is not { } s
            ? null
            : propertyName switch
            {
                nameof(Sequential.Remark) => s.Remark,
                nameof(Sequential.Layers) => s.Layers,
                nameof(Sequential.UsedCount) => s.UsedCount,
                nameof(Sequential.CreateTime) => s.CreateTime,
                nameof(Sequential.ContentJson) => s.ContentJson,
                _ => null
            };

    [HttpPost("Layers/All")]
    public JsonArray LayersAll(string sequentialName) =>
        App.Database.SequentialRecord.Find(sequentialName) is not { } s
            ? new()
            : new(JsonDocument.Parse(s.ContentJson).RootElement.EnumerateArray()
                .Select((je, index) =>
                {
                    var name = je.GetProperty("Name").GetString()!;
                    return (JsonNode)new JsonObject
                    {
                        ["key"] = index,
                        ["type"] = name.GetLayerType(),
                        ["name"] = name,
                        [nameof(Module.OutputChannels).ToCamel()] = je.GetDynamicProperty(nameof(Module.OutputChannels)),
                        [nameof(Conv2d.KernelSize).ToCamel()] = je.GetDynamicProperty(nameof(Conv2d.KernelSize)),
                        [nameof(Conv2d.Stride).ToCamel()] = je.GetDynamicProperty(nameof(Conv2d.Stride))
                    };
                }).ToArray());

    #region Find



    #endregion

}

