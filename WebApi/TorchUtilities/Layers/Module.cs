using TorchSharp;
using WebApi.TorchUtilities.Attributes;
using WebApi.TorchUtilities.Misc;

namespace WebApi.TorchUtilities.Layers;

public abstract class Module
{
    [DeserializerIgnore]
    public virtual Module Last { get; set; } = null!;

    [DeserializerIgnore]
    public virtual long InputChannels
    {
        get => Last.OutputChannels.TryGetValue;
        set => Last.OutputChannels = value;
    }

    public virtual Optional<long> OutputChannels { get; set; }

    public abstract torch.nn.Module ToTorch();
}
