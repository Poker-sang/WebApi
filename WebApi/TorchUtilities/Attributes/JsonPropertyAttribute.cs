namespace WebApi.TorchUtilities.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class JsonPropertyIgnoreAttribute : Attribute
{
    public JsonPropertyIgnoreAttribute(params string[] properties) => Properties = properties;

    public string[] Properties { get; }
}
