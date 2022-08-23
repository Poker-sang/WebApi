using WebApi.TorchUtilities.Layers;

namespace WebApi.TorchUtilities.Interfaces;

internal interface IDeserialize<out TSelf> where TSelf : Module, IDeserialize<TSelf>
{
    public static abstract TSelf Deserialize(System.Text.Json.JsonElement jsonElement);
    public System.Text.Json.Nodes.JsonArray ToJson();
}
