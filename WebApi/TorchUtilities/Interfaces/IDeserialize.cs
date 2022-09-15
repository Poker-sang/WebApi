using WebApi.TorchUtilities.Layers;

namespace WebApi.TorchUtilities.Interfaces;

internal interface IDeserialize<out TSelf> where TSelf : Module, IDeserialize<TSelf>
{
    /// <summary>
    /// 从<paramref name="jsonObject"/>反序列化得到对象
    /// </summary>
    public static abstract TSelf Deserialize(System.Text.Json.Nodes.JsonObject jsonObject);
    /// <summary>
    /// 对象转用于网络接口的json
    /// </summary>
    public System.Text.Json.Nodes.JsonArray ToWebJson();
    /// <summary>
    /// 对象转用于数据库的json（只转有不合默认值的项，即Changed <see langword="is true"/>）
    /// </summary>
    public System.Text.Json.Nodes.JsonObject ToSqlJson();
}
