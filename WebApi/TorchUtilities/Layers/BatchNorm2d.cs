using TorchSharp;
using WebApi.TorchUtilities.Attributes;
using WebApi.TorchUtilities.Interfaces;
using WebApi.TorchUtilities.Misc;

namespace WebApi.TorchUtilities.Layers;

[Deserializer]
public partial class BatchNorm2d : Module, IDeserialize<BatchNorm2d>
{
    [DeserializerIgnore]
    public Optional<long> Feature => InputChannels;

    [DeserializerIgnore]
    public override Optional<long> OutputChannels => InputChannels;

    public Optional<double> Eps { get; } = 1e-05d;
    public Optional<double> Momentum { get; } = 0.1d;
    public Optional<bool> Affine { get; } = true;
    public Optional<bool> TrackRunningStats { get; } = true;

    public override torch.nn.Module ToTorch() => torch.nn.BatchNorm2d(Feature, Eps, Momentum, Affine, TrackRunningStats);
}
