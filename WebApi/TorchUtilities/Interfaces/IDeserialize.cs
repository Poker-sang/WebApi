using WebApi.TorchUtilities.Layers;

namespace WebApi.TorchUtilities.Interfaces;

internal interface IDeserialize<out TSelf> where TSelf : Module, IDeserialize<TSelf>
{
    /// <summary>
    /// 从<paramref name="jsonObject"/>反序列化得到对象
    /// </summary>
    public static abstract TSelf Deserialize(System.Text.Json.Nodes.JsonObject jsonObject);
    /// <summary>
    /// 对象转json
    /// </summary>
    public System.Text.Json.Nodes.JsonArray ToFullJson();
    /// <summary>
    /// 对象转json（只转有不合默认值的项，即Changed <see langword="is true"/>）
    /// </summary>
    public System.Text.Json.Nodes.JsonArray ToJson();
}
