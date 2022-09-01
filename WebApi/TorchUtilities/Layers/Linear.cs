using TorchSharp;
using WebApi.TorchUtilities.Attributes;
using WebApi.TorchUtilities.Interfaces;
using WebApi.TorchUtilities.Misc;

namespace WebApi.TorchUtilities.Layers;

[Deserializer]
public partial class Linear : Module, IDeserialize<Linear>
{
    public Linear() { }
    public Optional<long> InputSize { get; set; } = Optional<long>.Default;
    public Optional<long> OutputSize { get; set; } = Optional<long>.Default;
    public Optional<bool> Bias { get; set; } = true;
    [DeserializerIgnore]
    public sealed override long InputChannels => 1;
    public sealed override Optional<long> OutputChannels => 1;

    public Linear(long inputSize, long outputSize)
    {
        InputSize = inputSize;
        OutputSize = outputSize;
    }

    public override torch.nn.Module ToTorch() => torch.nn.Linear(InputSize, OutputSize, Bias);
}
