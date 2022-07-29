using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using TorchSharp;

namespace WebApi.TorchUtilities.Layers;

public class Linear : Module
{
    public long InputSize { get; set; }
    public long OutputSize { get; set; }
    public bool Bias { get; set; } = true;
    [JsonIgnore]
    public sealed override long InputChannels => 1;
    public sealed override long OutputChannels => 1;

    public Linear(long inputSize, long outputSize)
    {
        InputSize = inputSize;
        OutputSize = outputSize;
    }

    public override torch.nn.Module ToTorch() => torch.nn.Linear(InputSize, OutputSize, Bias);
}
