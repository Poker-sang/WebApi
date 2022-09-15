using TorchSharp;
using WebApi.TorchUtilities.Attributes;
using WebApi.TorchUtilities.Interfaces;
using WebApi.TorchUtilities.Misc;

namespace WebApi.TorchUtilities.Layers;

// ReSharper disable once InconsistentNaming
[Deserializer]
public partial class ReLU : Module, IDeserialize<ReLU>
{
    public Optional<bool> InPlace { get; } = false;

    [DeserializerIgnore]
    public override Optional<long> OutputChannels => Last.OutputChannels;

    public override torch.nn.Module ToTorch() => torch.nn.ReLU(InPlace);
}
