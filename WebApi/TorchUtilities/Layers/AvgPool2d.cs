using TorchSharp;
using WebApi.TorchUtilities.Attributes;
using WebApi.TorchUtilities.Interfaces;
using WebApi.TorchUtilities.Misc;

namespace WebApi.TorchUtilities.Layers;

[Deserializer]
public partial class AvgPool2d : Module, IDeserialize<AvgPool2d>
{
    public AvgPool2d() { }

    [DeserializerIgnore]
    public override Optional<long> OutputChannels => Last.OutputChannels;

    public Optional<Rect> KernelSize { get; } = Optional<Rect>.Default;

    public Optional<Rect> Stride { get; } = (1, 1);

    public AvgPool2d(Rect kernelSize) => KernelSize = kernelSize;

    public override torch.nn.Module ToTorch() => torch.nn.AvgPool2d(KernelSize, Stride);
}
