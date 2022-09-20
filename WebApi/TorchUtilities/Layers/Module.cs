using TorchSharp;
using WebApi.TorchUtilities.Attributes;
using WebApi.TorchUtilities.Misc;

namespace WebApi.TorchUtilities.Layers;

public abstract class Module
{
    [DeserializerIgnore]
    public virtual Module Last { get; set; } = null!;

    [DeserializerIgnore]
    public virtual long InputChannels => Last.OutputChannels.Value;

    public virtual Optional<long> OutputChannels { get; } = Optional<long>.Default;

    public abstract torch.nn.Module ToTorch();
}
