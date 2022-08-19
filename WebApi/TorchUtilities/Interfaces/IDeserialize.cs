namespace WebApi.TorchUtilities.Interfaces;

internal interface IDeserialize<TSelf> where TSelf : IDeserialize<TSelf>
{
    public static abstract TSelf Deserialize(System.Text.Json.JsonElement jsonElement);
    public System.Text.Json.Nodes.JsonObject ToJson();
}
