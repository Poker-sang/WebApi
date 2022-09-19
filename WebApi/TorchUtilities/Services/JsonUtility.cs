using System.Text.Json.Nodes;
using WebApi.TorchUtilities.Layers;
using WebApi.TorchUtilities.Misc;

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

    /// <summary>
    /// 获取
    /// </summary>
    /// <param name="je"></param>
    /// <param name="property"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public static JsonNode? GetDynamicProperty<T>(this JsonObject je, string property) where T : notnull 
        => je.TryGetPropertyValue(property, out var result) && result is not null ? Optional<T>.FromJson(result).ToSqlJson() : null;

    /// <summary>
    /// 从<paramref name="jn"/>获取可选参数值
    /// <br/><inheritdoc cref="Misc.Optional{T}._binding"/>
    /// </summary>
    /// <param name="jn">提供<see cref="JsonValue"/>，<see cref="string"/>类型的<see cref="JsonNode"/></param>
    /// <param name="v">默认-2</param>
    /// <returns>是否是绑定参数</returns>
    public static bool TrySplitOptParam(this JsonNode jn, out int v)
    {
        v = -2;
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

    /// <summary>
    /// 从<paramref name="layer"/>获取里面包含的所有参数
    /// </summary>
    /// <param name="layer">表示一个层的参数</param>
    /// <returns>参数列表以<see cref="JsonArray"/>形式的参数</returns>
    public static JsonArray GetOptParams(this JsonObject layer)
    {
        var result = new SortedSet<int>();
        var hasAsterisk = false;
        foreach (var (_, value) in layer)
            if (value?.TrySplitOptParam(out var rst) is true)
                if (rst is -1)
                    hasAsterisk = true;
                else
                    _ = result.Add(rst);

        var ja = new JsonArray();
        if (hasAsterisk)
            ja.Add("*");
        foreach (var i in result)
            ja.Add(i);
        return ja;
    }
}
