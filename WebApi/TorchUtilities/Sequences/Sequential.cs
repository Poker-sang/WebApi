using System.Collections;
using System.Text.Json.Serialization;
using TorchSharp;
using WebApi.TorchUtilities.Layers;

namespace WebApi.TorchUtilities.Sequences;

public class Sequential : Module, IEnumerable<Module>
{
    private Module _last;
    public IEnumerator<Module> GetEnumerator() => Modules.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [JsonIgnore]
    public override Module Last
    {
        get => _last;
        set
        {
            if (Modules is { Count: not 0 })
                Modules[0].Last = value;
            _last = value;
        }
    }

    [JsonIgnore]
    public override long InputChannels
    {
        get => Modules is { Count: not 0 } ? Modules[0].InputChannels : -1;
        set => Modules[0].InputChannels = Modules is { Count: not 0 } ? value : throw new NotSupportedException();
    }

    public sealed override long OutputChannels
    {
        get => Modules is { Count: not 0 } ? Modules[^1].OutputChannels : InputChannels;
        set
        {
            if (Modules is { Count: not 0 })
                Modules[^1].OutputChannels = value;
            else
                InputChannels = value;
        }
    }

    [JsonIgnore]
    public List<Module> Modules { get; } = new();
    [JsonIgnore]
    public IEnumerable<Module> ModulesOutput => Modules.Where(t => t is not InputLayer);

    public Sequential(Module last = null!) => _last = last;

    public void Add(Module module)
    {
        module.Last = Modules.Count is not 0 ? Modules[^1] : Last;
        Modules.Add(module);
    }
    public void AddRange(params Module[] modules)
    {
        foreach (var module in modules)
            Add(module);
    }

    public override torch.nn.Module ToTorch()
        => torch.nn.Sequential(ModulesOutput.Select(t => t.ToTorch()));
}

