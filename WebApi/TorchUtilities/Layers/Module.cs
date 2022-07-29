using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using TorchSharp;

namespace WebApi.TorchUtilities.Layers;

public abstract class Module
{
    [JsonIgnore] 
    public virtual Module Last { get; set; } = null!;

    [JsonIgnore]
    public virtual long InputChannels
    {
        get => Last.OutputChannels;
        set => Last.OutputChannels = value;
    }

    public virtual long OutputChannels { get; set; }

    public abstract torch.nn.Module ToTorch();
}
