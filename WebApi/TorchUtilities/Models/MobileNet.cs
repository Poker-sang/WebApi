using TorchSharp;
using WebApi.TorchUtilities.Layers;
using WebApi.TorchUtilities.Sequences;

namespace WebApi.TorchUtilities.Models;

public class MobileNet : Net
{
    public MobileNet() : base("MobileNet") => Layers = new(new InputLayer(3))
        {
            new Conv2d(32, (3, 3)) { Stride = (2, 2), Padding = (Misc.PaddingType)(1, 1) },
            new BatchNorm2d(),
            new ReLU(),
            new Conv2dDw(64),
            new Conv2dDw(128) { Stride = (2, 2) },
            new Conv2dDw(128),
            new Conv2dDw(256) { Stride = (2, 2) },
            new Conv2dDw(256),
            new Conv2dDw(512) { Stride = (2, 2) },
            new Conv2dDw(512),
            new Conv2dDw(512),
            new Conv2dDw(512),
            new Conv2dDw(512),
            new Conv2dDw(512),
            new Conv2dDw(1024) { Stride = (2, 2) },
            new Conv2dDw(1024),
            new AvgPool2d((7, 7))
        };

    public Sequential Layers { get; }

    public override torch.Tensor Forward(torch.Tensor t)
    {
        t = Layers.ToTorch().forward(t);
        t = t.flatten(1);
        t = new Linear(1024, 10).ToTorch().forward(t);
        return t;
    }
}
