using System.Collections;
using TorchSharp;
using WebApi.TorchUtilities.Attributes;
using WebApi.TorchUtilities.Misc;

namespace WebApi.TorchUtilities.Layers;

public class Sequential : Module, IEnumerable<Module>
{
    private Module _last;
    public IEnumerator<Module> GetEnumerator() => Modules.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [DeserializerIgnore]
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

    [DeserializerIgnore]
    public override long InputChannels
    {
        get => Modules is { Count: not 0 } ? Modules[0].InputChannels : -1;
        set => Modules[0].InputChannels = Modules is { Count: not 0 } ? value : throw new NotSupportedException();
    }

    public sealed override Optional<long> OutputChannels
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

    [DeserializerIgnore]
    public List<Module> Modules { get; } = new();

    [DeserializerIgnore]
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

