using System.Text.Json.Serialization;
using TorchSharp;
using WebApi.TorchUtilities.Misc;

namespace WebApi.TorchUtilities.Layers;

public class AvgPool2d : Module
{
    [JsonIgnore]
    public override long OutputChannels
    {
        get => Last.OutputChannels;
        set => Last.OutputChannels = value;
    }
    public Rect KernelSize { get; set; }
    public Rect Stride { get; set; } = 1;

    public AvgPool2d(Rect kernelSize) => KernelSize = kernelSize;

    public override torch.nn.Module ToTorch() => torch.nn.AvgPool2d(KernelSize, Stride);
}
