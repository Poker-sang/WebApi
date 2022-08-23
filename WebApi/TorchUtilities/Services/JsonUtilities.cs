using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using TorchSharp;
using WebApi.TorchUtilities.Misc;
using WebApi.TorchUtilities.Sequences;

namespace WebApi.TorchUtilities.Services;

public static class JsonUtilities
{
    public static HashSet<string> PresetTypes { get; } = typeof(Module).Assembly.GetTypes().Where(t =>
            t.IsSubclassOf(typeof(Module)) && !t.IsSubclassOf(typeof(Sequential)))
        .Select(t => t.ToString()).ToHashSet();

    public static string GetLayerType(this string typeName) => PresetTypes.Contains(typeName) ? "normal" : "sequential";

    public static dynamic? GetDynamicProperty(this JsonElement je, string property) =>
        !je.TryGetProperty(property, out var result)
            ? null
            : result.ValueKind switch
            {
                JsonValueKind.Array => result.GetArrayLength() switch
                {
                    2 => result.GetRect().ToJson(),
                    _ => throw new NotSupportedException()
                },
                JsonValueKind.Number => result.GetInt64(),
                _ => result.GetString()
            };

    public static bool TrySplitOptParam(this JsonElement je, out int v)
    {
        v = 0;
        if (je is not { ValueKind: JsonValueKind.String }
            || je.GetString() is not { } str)
            return false;
        if (str is "*")
            v = -1;
        else if (int.TryParse(str[1..], out var rst))
            v = rst;
        return true;
    }

    public static JsonArray? GetOptParams(this JsonElement je)
    {
        var result = new SortedSet<int>();
        var hasAsterisk = false;
        foreach (var jsonProperty in je.EnumerateObject())
            if (jsonProperty.Value.TrySplitOptParam(out var rst))
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

    public static Database.Sequential? FindSequential(this string sequentialName) =>
        App.Database.SequentialRecord.Find(sequentialName);

    public static JsonNode? ToJsonNode(this object? obj) =>
        obj switch
        {
            null => null,
            long o => o,
            bool o => o,
            Rect o => o.ToJson(),
            PaddingModes o => JsonValue.Create(o),
            PaddingType o => o.ToJson(),
            _ => throw new InvalidDataException()
        };
}
