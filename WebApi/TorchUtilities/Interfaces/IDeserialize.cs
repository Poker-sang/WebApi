using WebApi.TorchUtilities.Layers;

namespace WebApi.TorchUtilities.Interfaces;

internal interface IDeserialize<out TSelf> where TSelf : Module, IDeserialize<TSelf>
{
    public static abstract TSelf Deserialize(System.Text.Json.Nodes.JsonObject jsonObject);
    public System.Text.Json.Nodes.JsonArray ToJson();
}
