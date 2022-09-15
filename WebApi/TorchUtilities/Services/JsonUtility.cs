﻿using System.Text.Json.Nodes;
using WebApi.TorchUtilities.Layers;

namespace WebApi.TorchUtilities.Services;

public static class JsonUtility
{

    public static string GetLayerTypeString(this string typeName) =>
        typeName.GetLayerType() switch
        {
            ModuleUtility.PresetType.Builtin => nameof(ModuleUtility.PresetType.Builtin).ToCamel(),
            ModuleUtility.PresetType.Sequential => nameof(ModuleUtility.PresetType.Sequential).ToCamel(),
            _ => throw new ArgumentOutOfRangeException()
        };

    public static dynamic? GetDynamicProperty(this JsonObject je, string property)
    {
        if (!je.TryGetPropertyValue(property, out var result))
            return null;
        switch (result)
        {
            case JsonArray arr:
                return arr.Count switch
                {
                    2 => arr.GetRect().ToJson(),
                    _ => throw new NotSupportedException()
                };
            case JsonValue val:
                if (val.TryGetValue<long>(out var l))
                    return l;
                return val.GetValue<string>();
            default: throw new NotSupportedException();
        }
    }

    public static bool TrySplitOptParam(this JsonNode jn, out int v)
    {
        v = 0;
        if (jn is not JsonValue jv)
            return false;
        if (!jv.TryGetValue<string>(out var str))
            return false;
        if (str is "*")
            v = -1;
        else if (int.TryParse(str[1..], out var rst))
            v = rst;
        else
            return false;
        return true;
    }

    public static JsonArray? GetOptParams(this JsonObject je)
    {
        var result = new SortedSet<int>();
        var hasAsterisk = false;
        foreach (var (_, value) in je)
            if (value?.TrySplitOptParam(out var rst) is true)
                if (rst is -1)
                    hasAsterisk = true;
                else
                    _ = result.Add(rst);

        if (result.Count is 0)
            return null;
        var ja = new JsonArray();
        if (hasAsterisk)
            ja.Add("*");
        foreach (var i in result)
            ja.Add(i);
        return ja;
    }
}