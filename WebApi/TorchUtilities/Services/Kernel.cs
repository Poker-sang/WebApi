using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using WebApi.TorchUtilities.Misc;
using WebApi.TorchUtilities.Sequences;

namespace WebApi.TorchUtilities.Services;

public static class Kernel
{
    public static Dictionary<string, IEnumerable<PropertyInfo>> Types => typeof(Kernel).Assembly.GetTypes()
        .Where(t => t.Namespace?.Contains($"{nameof(WebApi)}.{nameof(TorchUtilities)}.{nameof(Layers)}") ?? false)
        .ToDictionary(t => t.Name,
            t => t.GetProperties().Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() is null));

    public static JsonArray GetJson(this Sequential sequential)
        => new(sequential
            .Select(module =>
            {
                var name = module.GetType().Name;
                return (JsonNode)
                    new JsonObject(Types[name]
                        .Select(p => new KeyValuePair<string, JsonNode?>(p.Name,
                            (JsonNode?)(dynamic?)p.GetValue(module)))) { { "Name", name } };
            })
            .ToArray());


    public static HashSet<string> Preset { get; } = typeof(Module).Assembly.GetTypes().Where(t =>
            t.IsSubclassOf(typeof(Module)) && !t.IsSubclassOf(typeof(Sequences.Sequential)))
        .Select(t => t.ToString()).ToHashSet();

    public static string GetLayerType(this string typeName) => Preset.Contains(typeName) ? "normal" : "sequential";

    public static dynamic? GetDynamicProperty(this JsonElement je, string property)
        => !je.TryGetProperty(property, out var result)
            ? null
            : result.ValueKind switch
            {
                JsonValueKind.Array => result.GetArrayLength() switch
                {
                    2 => new Rect(result),
                    _ => throw new NotSupportedException()
                },
                JsonValueKind.Number => result.GetInt64(),
                _ => result.GetString()
            };
}
