global using Rect = System.ValueTuple<long, long>;

/*
public struct Rect : Tuple<int,int>
{
    public long Value1 { get; set; }
    public long Value2 { get; set; }

    public Rect(long v) => Value1 = Value2 = v;
    public Rect((long, long) v) => Value1 = v;
    public Rect(long v1, long v2)
    {
        Value1 = v1;
        Value2 = v2;
    }

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

    public object? this[int index] => throw new NotImplementedException();

    public int Length { get; }
}
*/
