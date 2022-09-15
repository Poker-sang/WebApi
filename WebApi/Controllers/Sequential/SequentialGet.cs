using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;
using WebApi.Database;
using WebApi.TorchUtilities.Layers;
using WebApi.TorchUtilities.Misc;
using WebApi.TorchUtilities.Services;

namespace WebApi.Controllers.Sequential;
public partial class SequentialController
{
    /// <returns><b>数据库</b>中全部的<see cref="SequentialRecord"/>及其元数据</returns>
    [HttpGet("Sequential/All")]
    public JsonArray All() =>
        new(_dbContext.SequentialRecord.ToArray()
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
    [HttpGet("Sequential/Find")]
    public async Task<JsonObject?> Find(string sequentialName) =>
        await FindSequential(sequentialName) is not { } s
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
    [HttpGet("Sequential/Find/Metadata")]
    [Obsolete("目前没用到")]
    public async Task<dynamic?> FindMetaData(string sequentialName, string metadataName) =>
        await FindSequential(sequentialName) is not { } s
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
    [HttpGet("Sequential/Params")]
    public async Task<JsonArray> Params(string sequentialName) =>
        await FindSequential(sequentialName) is not { } s
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

    /// <returns>所有用户自定义的<see cref="TorchUtilities.Layers.Sequential"/>）</returns>
    [HttpGet("Sequential/List")]
    public JsonArray LayersAll() =>
        new(_dbContext.SequentialRecord.ToArray().Select(t =>
            (JsonNode)new JsonObject
            {
                ["label"] = t.Name,
                ["value"] = t.Name
            }).ToArray());

    #region Layers

    /// <returns><b>数据库</b>中<see cref="SequentialRecord"/>包含的全部层及其被指定的参数</returns>
    [HttpGet("Layers/All")]
    public async Task<JsonArray> Layers(string sequentialName)
    {
        var items = (await ParseContentJson(sequentialName))?
            .Select((je, index) =>
            {
                var jo = je?.AsObject() ?? throw new NullReferenceException(nameof(je));
                var name = jo["Name"]?.AsValue().GetValue<string>() ?? throw new NullReferenceException("Name");
                return (JsonNode)new JsonObject
                {
                    ["key"] = index,
                    ["name"] = name,
                    ["type"] = name.GetLayerTypeString(),
                    ["containsParams"] = jo.GetOptParams(),
                    [nameof(Module.OutputChannels).ToCamel()] = jo.GetDynamicProperty(nameof(Module.OutputChannels)),
                    [nameof(Conv2d.KernelSize).ToCamel()] = jo.GetDynamicProperty(nameof(Conv2d.KernelSize)),
                    [nameof(Conv2d.Stride).ToCamel()] = jo.GetDynamicProperty(nameof(Conv2d.Stride))
                };
            })?.ToArray();

        return items is null ? new() : new(items);
    }

    /// <returns><see cref="TorchUtilities.Layers.Sequential"/>中指定层的<b>全部</b>参数、包括默认参数</returns>
    [HttpGet("Layers/Edit")]
    public async Task<IEnumerable<JsonArray>?> LayersEdit(string sequentialName)
    {
        var tasks = (await ParseContentJson(sequentialName))?
            .Select(async jn =>
            {
                var layer = jn?.AsObject() ?? throw new NullReferenceException(nameof(jn));
                var name = layer["Name"]?.AsValue().GetValue<string>() ??
                           throw new NullReferenceException(nameof(layer));
                if (name switch
                {
                    nameof(Conv2d) => Conv2d.Deserialize(layer).ToWebJson(),
                    nameof(AvgPool2d) => AvgPool2d.Deserialize(layer).ToWebJson(),
                    nameof(BatchNorm2d) => BatchNorm2d.Deserialize(layer).ToWebJson(),
                    nameof(Linear) => Linear.Deserialize(layer).ToWebJson(),
                    nameof(Flatten) => Flatten.Deserialize(layer).ToWebJson(),
                    nameof(ReLU) => ReLU.Deserialize(layer).ToWebJson(),
                    _ => null
                } is { } ja)
                    return ja;

                if (await FindSequential(name) is not { } s2)
                    throw new InvalidDataException();

                var p = new Dictionary<string, (Type Type, object? Value)>(s2.GetParams()
                    .Select(t => new KeyValuePair<string, (Type, object?)>(t.Name, (t.Type, t.Default))));
                foreach (var jp in layer.Where(jp => p.ContainsKey(jp.Key)))
                    p[jp.Key] = (p[jp.Key].Type, Optional.FromJson(p[jp.Key].Type, jp.Value!));

                foreach (var (key, (type, value)) in p)
                    if (value is null)
                        throw new NullReferenceException(nameof(value));
                    else if (value.GetType().GetGenericTypeDefinition() != typeof(Optional<>))
                        p[key] = (type, Optional.FromJson(type, value));

                return new(p.Select(pair =>
                    (JsonNode)((Optional<object>)pair.Value.Value!).ToWebJson(pair.Value.Type, pair.Key)).ToArray());
            });
        return tasks is null ? null : (IEnumerable<JsonArray>)await Task.WhenAll(tasks);
    }

    /// <returns><see cref="TorchUtilities.Layers.Sequential"/>中指定层的全部默认参数、及其被指定的参数</returns>
    [HttpGet("Layers/Default")]
    public async Task<JsonObject?> LayersDefault(string layerName)
    {
        JsonArray arr;
        if (layerName switch
        {
            nameof(Conv2d) => new Conv2d().ToWebJson(),
            nameof(AvgPool2d) => new AvgPool2d().ToWebJson(),
            nameof(BatchNorm2d) => new BatchNorm2d().ToWebJson(),
            nameof(Linear) => new Linear().ToWebJson(),
            nameof(Flatten) => new Flatten().ToWebJson(),
            nameof(ReLU) => new ReLU().ToWebJson(),
            _ => null
        } is { } ja)
            arr = ja;
        else
        {
            if (await FindSequential(layerName) is not { } s)
                return null;

            var p = new Dictionary<string, (Type Type, object Value)>(s.GetParams()
                .Select(t => t.Default is not null && t.Type.GetGenericTypeDefinition() != typeof(Optional<>)
                    ? new(t.Name, (t.Type, Optional.FromJson(t.Type, t.Default)))
                    : new KeyValuePair<string, (Type, object)>(t.Name,
                        (t.Type, Optional<object>.Default))));

            arr = new(p.Select(pair => (JsonNode)((Optional<object>)pair.Value.Value).ToWebJson(pair.Value.Type, pair.Key))
                .ToArray());
        }

        return new()
        {
            ["desc"] = new JsonObject
            {
                ["key"] = 0,
                ["name"] = layerName,
                ["type"] = layerName.GetLayerTypeString(),
                ["containsParams"] = null,
                [nameof(Module.OutputChannels).ToCamel()] = null,
                [nameof(Conv2d.KernelSize).ToCamel()] = null,
                [nameof(Conv2d.Stride).ToCamel()] = null
            },
            ["params"] = arr
        };
    }


    /// <returns>所有基础的类型（继承于<see cref="Module"/>但不继承于<see cref="TorchUtilities.Layers.Sequential"/>）</returns>
    [HttpGet("Layers/List")]
    public JsonArray LayersNew() =>
        new(ModuleUtility.PresetTypes.Select(t =>
            (JsonNode)new JsonObject
            {
                ["label"] = t,
                ["value"] = t
            }).ToArray());

    #endregion
}
