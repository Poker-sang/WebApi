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
    public JsonArray All() =>
        new(App.Database.SequentialRecord.ToArray()
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

    #region Find

    [HttpPost("Find")]
    public JsonObject? Find(string sequentialName) =>
        sequentialName.FindSequential() is not { } s
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
        sequentialName.FindSequential() is not { } s
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

    #endregion

    [HttpPost("Params")]
    public JsonArray Params(string sequentialName) =>
        sequentialName.FindSequential() is not { } s
            ? new()
            : new(s.GetParams()
                .Select((param, index) =>
                    (JsonNode)new JsonObject
                    {
                        ["key"] = index,
                        [nameof(ParamUtilities.Param.Name).ToCamel()] = param.Name,
                        [nameof(ParamUtilities.Param.Type).ToCamel()] = param.Type.GetName(),
                        [nameof(ParamUtilities.Param.Remark).ToCamel()] = param.Remark,
                        [nameof(ParamUtilities.Param.Default).ToCamel()] = param.Default.ToJsonNode()
                    }).ToArray());

    #region Layers

    [HttpPost("Layers")]
    public JsonArray Layers(string sequentialName) =>
        sequentialName.FindSequential() is not { } s
            ? new()
            : new(JsonDocument.Parse(s.ContentJson).RootElement.EnumerateArray()
                .Select((je, index) =>
                {
                    var name = je.GetProperty("Name").GetString()!;
                    return (JsonNode)new JsonObject
                    {
                        ["key"] = index,
                        ["name"] = name,
                        ["type"] = name.GetLayerType(),
                        ["containsParams"] = je.GetOptParams(),
                        [nameof(Module.OutputChannels).ToCamel()] = je.GetDynamicProperty(nameof(Module.OutputChannels)),
                        [nameof(Conv2d.KernelSize).ToCamel()] = je.GetDynamicProperty(nameof(Conv2d.KernelSize)),
                        [nameof(Conv2d.Stride).ToCamel()] = je.GetDynamicProperty(nameof(Conv2d.Stride))
                    };
                }).ToArray());

    [HttpPost("Layers/Edit")]
    public JsonArray? LayersEdit(string sequentialName, int index)
    {
        if (sequentialName.FindSequential() is not { } s)
            return null;

        var arr = JsonDocument.Parse(s.ContentJson).RootElement.EnumerateArray().ToArray();
        if (arr.Length <= index)
            return null;
        var layer = arr[index];
        return layer.GetProperty("Name").GetString()! switch
        {
            nameof(Conv2d) => Conv2d.Deserialize(layer).ToJson(),
            nameof(AvgPool2d) => AvgPool2d.Deserialize(layer).ToJson(),
            nameof(BatchNorm2d) => BatchNorm2d.Deserialize(layer).ToJson(),
            nameof(Linear) => Linear.Deserialize(layer).ToJson(),
            nameof(ReLU) => ReLU.Deserialize(layer).ToJson(),
            _ => null
        };
    }

    #endregion

}

