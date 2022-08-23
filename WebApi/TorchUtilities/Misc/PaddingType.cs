using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using TorchSharp;
using WebApi.TorchUtilities.Services;

namespace WebApi.TorchUtilities.Misc;

public struct PaddingType : IParsable<PaddingType>
{
    private Rect? _rect;
    private Padding? _padding;

    public PaddingType()
    {
        _rect = (0, 0);
        _padding = null;
    }
    public PaddingType(Rect r) => _rect = r;
    public PaddingType(Padding p) => _padding = p;

    public static implicit operator PaddingType(Rect t) => new(t);
    public static implicit operator PaddingType(Padding t) => new(t);
    public static implicit operator PaddingType(int t) => new((Padding)t);
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

    public void Set(Padding t2)
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

    public Padding? GetPadding() => _padding;
    public bool TryGetPadding([NotNullWhen(true)] out Padding? value)
    {
        value = _padding;
        return _padding is not null;
    }

    public static PaddingType Parse(string s, IFormatProvider? provider = null)
    {
        if (int.TryParse(s, out var result1))
            return result1;
        if (s.RectTryParse(out var result2))
            return result2;
        throw new FormatException();
    }

    public static bool TryParse(string? s, IFormatProvider? provider, out PaddingType result)
    {
        result = default;
        if (s is null)
            return false;

        if (int.TryParse(s, out var result1))
        {
            result = result1;
            return true;
        }
        if (s.RectTryParse(out var result2))
        {
            result = result2;
            return true;
        }

        return false;
    }
}
