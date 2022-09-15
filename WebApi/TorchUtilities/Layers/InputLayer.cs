using TorchSharp;
using WebApi.TorchUtilities.Attributes;
using WebApi.TorchUtilities.Misc;

namespace WebApi.TorchUtilities.Layers;

//TODO
public sealed class InputLayer : Module
{
    [DeserializerIgnore]
    public override long InputChannels { get; } = new();
    public override Optional<long> OutputChannels => InputChannels;

    public InputLayer(long inOutputChannels) { } //=> OutputChannels = inOutputChannels;
    public override torch.nn.Module ToTorch() => throw new NotSupportedException();
}
