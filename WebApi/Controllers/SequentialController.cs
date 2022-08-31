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
    [HttpGet("All")]
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
    [HttpGet("Find")]
    public async Task<JsonObject?> Find(string sequentialName) =>
        await sequentialName.FindSequential() is not { } s
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
    [HttpGet("Find/Metadata")]
    public async Task<dynamic?> FindMetaData(string sequentialName, string metadataName) =>
        await sequentialName.FindSequential() is not { } s
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
    [HttpGet("Params")]
    public async Task<JsonArray> Params(string sequentialName) =>
        await sequentialName.FindSequential() is not { } s
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
    [HttpGet("Layers")]
    public async Task<JsonArray> Layers(string sequentialName) =>
        await sequentialName.FindSequential() is not { } s
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
    [HttpGet("Layers/Edit")]
    public async Task<JsonArray?> LayersEdit(string sequentialName, int index)
    {
        if (await sequentialName.FindSequential() is not { } s)
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

        if (await name.FindSequential() is not { } s2)
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

    /// <returns>所有用户自定义的<see cref="Sequential"/>）</returns>
    [HttpGet("Layers/All")]
    public JsonArray LayersAll() =>
        new(App.Database.SequentialRecord.ToArray().Select(t =>
            (JsonNode)new JsonObject
            {
                ["label"] = t.Name,
                ["value"] = t.Name
            }).ToArray());

    /// <returns>所有基础的类型（继承于<see cref="Module"/>但不继承于<see cref="Sequential"/>）</returns>
    [HttpGet("Layers/New")]
    public JsonArray LayersNew() =>
        new(JsonUtilities.PresetTypes.Select(t =>
            (JsonNode)new JsonObject
            {
                ["label"] = t,
                ["value"] = t
            }).ToArray());

    #endregion
}

