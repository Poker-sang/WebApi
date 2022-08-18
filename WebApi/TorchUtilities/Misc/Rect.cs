using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace WebApi.TorchUtilities.Misc;

public struct Rect
{
    public (long, long) Value { get; set; }

    public Rect(long v) => Value = (v, v);
    public Rect((long, long) v) => Value = v;
    public Rect(long v1, long v2) => Value = (v1, v2);
    public Rect(JsonElement je) => Value = (je[0].GetInt64(), je[1].GetInt64());

    public static implicit operator Rect(long v) => new(v);
    public static implicit operator Rect((long, long) v) => new(v);
    public static implicit operator (long, long)(Rect r) => r.Value;
    public static implicit operator JsonArray(Rect r) => new(r.Value.Item1, r.Value.Item1);
    public override string ToString() => $"[{Value.Item1},{Value.Item2}]";



    public static bool TryParse([NotNullWhen(true)] string? s, out Rect result)
    {
        if (s is not null)
            if (Regex.Match(s, @"^\[(.+),(.+)\]$") is { Success: true } match)
                if (long.TryParse(match.Groups[1].Value, out var number1) &&
                    long.TryParse(match.Groups[2].Value, out var number2))
                {
                    result = new(number1, number2);
                    return true;
                }

        result = default;
        return false;
    }
}
