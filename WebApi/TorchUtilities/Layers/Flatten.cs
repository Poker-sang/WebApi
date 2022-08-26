using TorchSharp;
using WebApi.TorchUtilities.Attributes;
using WebApi.TorchUtilities.Interfaces;
using WebApi.TorchUtilities.Misc;

namespace WebApi.TorchUtilities.Layers;

[Deserializer]
public partial class Flatten : Module, IDeserialize<Flatten>
{
    public Optional<long> StartDimension { get; set; } = 1;
    public Optional<long> EndDimension { get; set; } = -1;

    [DeserializerIgnore]
    public override Optional<long> OutputChannels
    {
        get => StartDimension.TryGetValue is 1 && EndDimension.TryGetValue is -1 or 3
            ? 0
            : throw new InvalidDataException();
        set => throw new InvalidOperationException();
    }

    public override torch.nn.Module ToTorch() => torch.nn.Flatten(StartDimension, EndDimension);
}
