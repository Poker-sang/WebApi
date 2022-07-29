using System.Text.Json.Serialization;
using TorchSharp;

namespace WebApi.TorchUtilities.Layers;

public sealed class InputLayer : Module
{
    [JsonIgnore]
    public override long InputChannels { get; set; }
    public override long OutputChannels { get => InputChannels; set => InputChannels = value; }

    public InputLayer(long inOutputChannels) => OutputChannels = inOutputChannels;
    public override torch.nn.Module ToTorch() => throw new NotSupportedException();
}
