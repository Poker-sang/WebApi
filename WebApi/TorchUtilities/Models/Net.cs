using System.Text.Json.Serialization;
using TorchSharp;

namespace WebApi.TorchUtilities.Models;

public abstract class Net : torch.nn.Module
{
    [JsonIgnore]
    public override bool training => true;

    protected Net(string name) : base(name)
    {
    }

    public abstract torch.Tensor Forward(torch.Tensor t);

    public override torch.Tensor forward(torch.Tensor t) => Forward(t);
}
