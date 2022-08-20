using TorchSharp;
using WebApi.TorchUtilities.Attributes;
using WebApi.TorchUtilities.Misc;

namespace WebApi.TorchUtilities.Layers;

public sealed class InputLayer : Module
{
    [DeserializerIgnore]
    public override long InputChannels { get; set; }
    public override Optional<long> OutputChannels { get => InputChannels; set => InputChannels = value; }

    public InputLayer(long inOutputChannels) => OutputChannels = inOutputChannels;
    public override torch.nn.Module ToTorch() => throw new NotSupportedException();
}
