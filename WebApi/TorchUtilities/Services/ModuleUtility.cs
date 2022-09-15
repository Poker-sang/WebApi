using WebApi.TorchUtilities.Layers;

namespace WebApi.TorchUtilities.Services;

public static class ModuleUtility
{
    public static HashSet<string> PresetTypes { get; } = typeof(Module).Assembly.GetTypes().Where(t =>
            t.IsSubclassOf(typeof(Module)) && !t.IsAssignableFrom(typeof(Sequential)) && !t.IsAssignableFrom(typeof(InputLayer)))
        .Select(t => t.Name).ToHashSet();

    public static PresetType GetLayerType(this string typeName) => PresetTypes.Contains(typeName)
        ? PresetType.Builtin
        : PresetType.Sequential;

    public enum PresetType
    {
        Builtin, Sequential
    }
}
