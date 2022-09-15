using TorchSharp;
using WebApi.TorchUtilities.Attributes;
using WebApi.TorchUtilities.Interfaces;
using WebApi.TorchUtilities.Misc;

namespace WebApi.TorchUtilities.Layers;

[Deserializer]
public partial class Flatten : Module, IDeserialize<Flatten>
{
    public Optional<long> StartDimension { get; } = 1;
    public Optional<long> EndDimension { get; } = -1;

    [DeserializerIgnore]
    public override Optional<long> OutputChannels =>
        StartDimension.GetValue() is 1 && EndDimension.GetValue() is -1 or 3
            ? 0
            : throw new InvalidDataException();

    public override torch.nn.Module ToTorch() => torch.nn.Flatten(StartDimension, EndDimension);
}
