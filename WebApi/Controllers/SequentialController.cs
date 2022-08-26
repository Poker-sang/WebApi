using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Nodes;
using WebApi.Database;
using WebApi.TorchUtilities.Layers;
using WebApi.TorchUtilities.Misc;
using WebApi.TorchUtilities.Services;

namespace WebApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class SequentialController : ControllerBase
{
    /// <returns><b>数据库</b>中全部的<see cref="SequentialRecord"/>及其元数据</returns>
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

    /// <returns><b>数据库</b>中<see cref="SequentialRecord"/>的全部元数据</returns>
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

    /// <returns><b>数据库</b>中<see cref="SequentialRecord"/>的指定元数据</returns>
    [HttpPost("Find/Metadata")]
    public dynamic? FindMetaData(string sequentialName, string metadataName) =>
        sequentialName.FindSequential() is not { } s
            ? null
            : metadataName switch
            {
                nameof(SequentialRecord.Remark) => s.Remark,
                nameof(SequentialRecord.Layers) => s.Layers,
                nameof(SequentialRecord.UsedCount) => s.UsedCount,
                nameof(SequentialRecord.CreateTime) => s.CreateTime,
                nameof(SequentialRecord.ContentJson) => s.ContentJson,
                _ => null
            };

    #endregion

    /// <returns><b>数据库</b>中<see cref="SequentialRecord"/>的参数</returns>
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

    /// <returns><b>数据库</b>中<see cref="SequentialRecord"/>包含的全部层及其被指定的参数</returns>
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

    /// <returns><see cref="Sequential"/>中指定层的<b>全部</b>参数、包括默认参数</returns>
    [HttpPost("Layers/Edit")]
    public JsonArray? LayersEdit(string sequentialName, int index)
    {
        if (sequentialName.FindSequential() is not { } s)
            return null;

        var arr = JsonDocument.Parse(s.ContentJson).RootElement.EnumerateArray().ToArray();
        if (arr.Length <= index)
            return null;
        var layer = arr[index];
        var name = layer.GetProperty("Name").GetString()!;
        if (name switch
        {
            nameof(Conv2d) => Conv2d.Deserialize(layer).ToJson(),
            nameof(AvgPool2d) => AvgPool2d.Deserialize(layer).ToJson(),
            nameof(BatchNorm2d) => BatchNorm2d.Deserialize(layer).ToJson(),
            nameof(Linear) => Linear.Deserialize(layer).ToJson(),
            nameof(Flatten) => Flatten.Deserialize(layer).ToJson(),
            nameof(ReLU) => ReLU.Deserialize(layer).ToJson(),
            _ => null
        } is { } ja)
            return ja;

        if (name.FindSequential() is not { } s2)
            return null;

        var p = new Dictionary<string, (Type Type, object Value)>(s2.GetParams()
            .Select(t => new KeyValuePair<string, (Type, object)>(t.Name, (t.Type, t.Default))));
        foreach (var jp in layer.EnumerateObject().Where(jp => p.ContainsKey(jp.Name)))
            p[jp.Name] = (p[jp.Name].Type, Optional.FromJson(p[jp.Name].Type, jp.Value));

        foreach (var (key, (type, value)) in p)
            if (value.GetType().GetGenericTypeDefinition() != typeof(Optional<>))
                p[key] = (type, Optional.FromJson(type, value));

        return new(p.Select(pair => (JsonNode)((Optional<object>)pair.Value.Value).ToJson(pair.Value.Type, pair.Key)).ToArray());
    }

    #endregion
}

