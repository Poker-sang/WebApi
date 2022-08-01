using System.Text.Json.Serialization;
using WebApi.TorchUtilities.Attributes;
using WebApi.TorchUtilities.Layers;
using WebApi.TorchUtilities.Misc;

namespace WebApi.TorchUtilities.Sequences;

[JsonPropertyIgnore(nameof(OutputChannels), nameof(Stride))]
public class Conv2dDw : Sequential
{
    private Rect _stride;

    public Rect Stride
    {
        get => _stride;
        set
        {
            _stride = value;
            First.Stride = value;
        }
    }

    /// <exception cref="IndexOutOfRangeException"/>
    [JsonIgnore]
    private Conv2d First => (Conv2d)Modules[0];

    [JsonIgnore]
    public sealed override long InputChannels
    {
        get => First.InputChannels;
        set
        {
            First.InputChannels = value;
            First.Groups = value;
        }
    }

    public Conv2dDw(long outputChannels) =>
        AddRange(
            new Conv2d(-1, 3) { Stride = Stride, Padding = 1, Groups = -1, Bias = false },
            new BatchNorm2d(),
            new ReLU { InPlace = true },
            new Conv2d(outputChannels, 1) { Bias = false },
            new BatchNorm2d(),
            new ReLU { InPlace = true });
}
