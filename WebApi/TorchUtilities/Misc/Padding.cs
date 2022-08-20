using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

namespace WebApi.TorchUtilities.Misc;

public struct PaddingType
{
    private Rect? _rect;
    private TorchSharp.Padding? _padding;

    public PaddingType()
    {
        _rect = (0, 0);
        _padding = null;
    }
    public PaddingType(Rect r) => _rect = r;
    public PaddingType(TorchSharp.Padding p) => _padding = p;

    public static implicit operator PaddingType(Rect t) => new(t);
    public static implicit operator PaddingType(TorchSharp.Padding t) => new(t);
    public static implicit operator PaddingType(int t) => new((TorchSharp.Padding)t);
    public static implicit operator PaddingType((int, int) t) => new(t);
    public static implicit operator JsonNode(PaddingType p) => p.ToJson();

    public JsonNode ToJson() =>
        _rect is null
            ? JsonValue.Create((int)_padding!)
            : new JsonArray(_rect.Value.Item1, _rect.Value.Item2);

    public void Set(Rect t1)
    {
        _padding = null;
        _rect = t1;
    }

    public void Set(TorchSharp.Padding t2)
    {
        _rect = null;
        _padding = t2;
    }
    public Rect? GetRect() => _rect;
    public bool TryGetRect([NotNullWhen(true)] out Rect? value)
    {
        value = _rect;
        return _rect is not null;
    }

    public TorchSharp.Padding? GetPadding() => _padding;
    public bool TryGetPadding([NotNullWhen(true)] out TorchSharp.Padding? value)
    {
        value = _padding;
        return _padding is not null;
    }
}
