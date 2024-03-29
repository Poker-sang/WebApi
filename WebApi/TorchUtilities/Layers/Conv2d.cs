﻿using TorchSharp;
using WebApi.TorchUtilities.Attributes;
using WebApi.TorchUtilities.Interfaces;
using WebApi.TorchUtilities.Misc;

namespace WebApi.TorchUtilities.Layers;

[Deserializer]
public partial class Conv2d : Module, IDeserialize<Conv2d>
{
    public Conv2d() { }

    public Conv2d(long outputChannel, Rect kernelSize)
    {
        OutputChannels = outputChannel;
        KernelSize = kernelSize;
    }

    public sealed override Optional<long> OutputChannels { get; } = Optional<long>.Default;
    public Optional<Rect> KernelSize { get; } = Optional<Rect>.Default;
    public Optional<Rect> Stride { get; } = (1, 1);
    public Optional<PaddingType> Padding { get; } = new PaddingType();
    public Optional<Rect> Dilation { get; } = (1, 1);
    public Optional<PaddingModes> PaddingMode { get; } = PaddingModes.Zeros;
    public Optional<long> Groups { get; } = 1;
    public Optional<bool> Bias { get; } = true;

    public override torch.nn.Module ToTorch() =>
        ((PaddingType)Padding).GetPadding() is { } paddingType
            ? torch.nn.Conv2d(InputChannels, OutputChannels, KernelSize, paddingType, Stride, Dilation, PaddingMode, Groups, Bias)
            : torch.nn.Conv2d(InputChannels, OutputChannels, KernelSize, Stride, ((PaddingType)Padding).GetRect(), Dilation, PaddingMode, Groups, Bias);
}
