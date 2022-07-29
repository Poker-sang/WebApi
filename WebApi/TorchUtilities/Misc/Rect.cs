using System.Text.Json.Nodes;

namespace WebApi.TorchUtilities.Misc;

public struct Rect
{
    public (long, long) Value { get; set; }

    public Rect(long v) => Value = (v, v);
    public Rect((long, long) v) => Value = v;

    public static implicit operator Rect(long v) => new(v);
    public static implicit operator Rect((long, long) v) => new(v);
    public static implicit operator (long, long)(Rect r) => r.Value;
    public static implicit operator JsonArray(Rect r) => new(r.Value.Item1, r.Value.Item2);
}
