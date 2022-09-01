global using Rect = System.ValueTuple<long, long>;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using TorchSharp;
using WebApi.TorchUtilities.Misc;

namespace WebApi.TorchUtilities.Services;

public static class TypeUtilities
{
    public static Rect GetRect(this JsonElement je) => new(je[0].GetInt64(), je[1].GetInt64());
    public static PaddingType GetPadding(this JsonElement je) => je.ValueKind is JsonValueKind.Array ? je.GetRect() : je.GetUInt16();

    public static JsonArray ToJson(this Rect r) => new(r.Item1, r.Item2);
    public static Rect RectParse(this string s)
    {
        if (Regex.Match(s, @"^\[(.+),(.+)\]$") is { Success: true } match &&
            long.TryParse(match.Groups[1].Value, out var number1) &&
            long.TryParse(match.Groups[2].Value, out var number2))
            return new(number1, number2);

        throw new FormatException();
    }
    public static bool RectTryParse([NotNullWhen(true)] this string? s, out Rect result)
    {
        if (s is not null &&
            Regex.Match(s, @"^\[(.+),(.+)\]$") is { Success: true } match &&
            long.TryParse(match.Groups[1].Value, out var number1) &&
            long.TryParse(match.Groups[2].Value, out var number2))
        {
            result = new(number1, number2);
            return true;
        }

        result = default;
        return false;
    }

    public static Type ParseType(this string s) =>
        s switch
        {
            nameof(Int64) => typeof(long),
            nameof(Boolean) => typeof(bool),
            nameof(Rect) => typeof(Rect),
            nameof(PaddingType) => typeof(PaddingType),
            nameof(PaddingModes) => typeof(PaddingModes),
            _ => throw new InvalidDataException(s)
        };

    public static string GetName(this Type t) => t == typeof(Rect) ? nameof(Rect) : t.Name;

    public static dynamic? ParseValue<T>(this string s) => s.ParseValue(typeof(T));

    public static dynamic? ParseValue(this string s, Type type) =>
        0 switch
        {
            0 when s is "null" => null,
            0 when type == typeof(long) => long.Parse(s),
            0 when type == typeof(bool) => bool.Parse(s),
            0 when type == typeof(Rect) => s.RectParse(),
            0 when type == typeof(PaddingType) => PaddingType.Parse(s),
            0 when type == typeof(PaddingModes) => Enum.Parse<PaddingModes>(s),
            _ => throw new InvalidDataException()
        };

    public static JsonNode? ToJsonNode(this object? obj) =>
        obj switch
        {
            null => null,
            long o => o,
            bool o => o,
            Rect o => o.ToJson(),
            PaddingModes o => JsonValue.Create(o),
            PaddingType o => o.ToJson(),
            _ => throw new InvalidDataException()
        };
}
