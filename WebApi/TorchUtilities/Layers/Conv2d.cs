using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using TorchSharp;
using WebApi.TorchUtilities.Attributes;
using WebApi.TorchUtilities.Interfaces;
using WebApi.TorchUtilities.Misc;

namespace WebApi.TorchUtilities.Layers;

[Deserializer]
public partial class Conv2d : Module
{
    private Conv2d() { }

    public Conv2d(long outputChannel, Rect kernelSize)
    {
        OutputChannels = outputChannel;
        KernelSize = kernelSize;
    }

    public sealed override long OutputChannels { get; set; }
    public Rect KernelSize { get; set; }
    public Rect Stride { get; set; } = 1;
    public Rect Padding { get; set; } = 0;
    public Padding? PaddingType { get; set; }
    public Rect Dilation { get; set; } = 1;
    public PaddingModes PaddingMode { get; set; } = PaddingModes.Zeros;
    public long Groups { get; set; } = 1;
    public bool Bias { get; set; } = true;

    public override torch.nn.Module ToTorch() =>
        PaddingType is { } paddingType
            ? torch.nn.Conv2d(InputChannels, OutputChannels, KernelSize, paddingType, Stride, Dilation, PaddingMode,
                Groups, Bias)
            : torch.nn.Conv2d(InputChannels, OutputChannels, KernelSize, Stride, Padding, Dilation, PaddingMode,
                Groups, Bias);
}
