using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using TorchSharp;

namespace WebApi.TorchUtilities.Layers;

public class BatchNorm2d : Module
{
    public long Feature
    {
        get => InputChannels;
        set => InputChannels = value;
    }

    [JsonIgnore]
    public override long OutputChannels
    {
        get => InputChannels;
        set => InputChannels = value;
    }

    public double Eps { get; set; } = 1e-05d;
    public double Momentum { get; set; } = 0.1d;
    public bool Affine { get; set; } = true;
    public bool TrackRunningStats { get; set; } = true;

    public override torch.nn.Module ToTorch() => torch.nn.BatchNorm2d(Feature, Eps, Momentum, Affine, TrackRunningStats);
}
