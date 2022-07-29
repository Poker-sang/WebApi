using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
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
    {
        return new(sequential
            .Select(module =>
            {
                var name = module.GetType().Name;
                return (JsonNode)
                    new JsonObject(Types[name]
                        .Select(p => new KeyValuePair<string, JsonNode?>(p.Name,
                            (JsonNode?)(dynamic?)p.GetValue(module))))
                    {
                        { "Name", name }
                    };
            })
            .ToArray());
    }
}
