using TorchSharp;
using WebApi.TorchUtilities.Attributes;

namespace WebApi.TorchUtilities.Layers;

public abstract class Module
{
    [DeserializerIgnore]
    public virtual Module Last { get; set; } = null!;

    [DeserializerIgnore]
    public virtual long InputChannels
    {
        get => Last.OutputChannels;
        set => Last.OutputChannels = value;
    }

    public virtual long OutputChannels { get; set; }

    public abstract torch.nn.Module ToTorch();
}
