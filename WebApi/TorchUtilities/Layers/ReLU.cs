using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using TorchSharp;

namespace WebApi.TorchUtilities.Layers;

// ReSharper disable once InconsistentNaming
public class ReLU : Module
{
    public bool InPlace { get; set; }

    [JsonIgnore]
    public override long OutputChannels
    {
        get => Last.OutputChannels;
        set => Last.OutputChannels = value;
    }

    public override torch.nn.Module ToTorch() => torch.nn.ReLU(InPlace);
}
