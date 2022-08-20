using TorchSharp;
using WebApi.TorchUtilities.Attributes;
using WebApi.TorchUtilities.Interfaces;
using WebApi.TorchUtilities.Misc;

namespace WebApi.TorchUtilities.Layers;

[Deserializer]
public partial class BatchNorm2d : Module, IDeserialize<BatchNorm2d>
{
    [DeserializerIgnore]
    public Optional<long> Feature
    {
        get => InputChannels;
        set => InputChannels = value;
    }

    [DeserializerIgnore]
    public override Optional<long> OutputChannels
    {
        get => InputChannels;
        set => InputChannels = value;
    }

    public Optional<double> Eps { get; set; } = 1e-05d;
    public Optional<double> Momentum { get; set; } = 0.1d;
    public Optional<bool> Affine { get; set; } = true;
    public Optional<bool> TrackRunningStats { get; set; } = true;

    public override torch.nn.Module ToTorch() => torch.nn.BatchNorm2d(Feature, Eps, Momentum, Affine, TrackRunningStats);
}
