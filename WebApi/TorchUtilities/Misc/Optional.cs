using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using WebApi.TorchUtilities.Services;

namespace WebApi.TorchUtilities.Misc;

public struct Optional<T>
{
    private readonly T? _value;
    public int? Binding { get; set; }

    public Optional() { }
    public Optional(T? value) => _value = value;

    public static implicit operator Optional<T>(T v) => new(v);
    public static implicit operator T(Optional<T> v) => v._value ?? throw new InvalidDataException();

    public T? TryGetValue => _value ?? default;

    public static Optional<T> FromJson(JsonElement je) =>
        je.TrySplitOptParam(out var rst)
            ? new() { Binding = rst }
            : new((T)(0 switch
            {
                0 when typeof(T) == typeof(sbyte) => je.GetSByte(),
                0 when typeof(T) == typeof(byte) => je.GetByte(),
                0 when typeof(T) == typeof(short) => je.GetInt16(),
                0 when typeof(T) == typeof(ushort) => je.GetUInt16(),
                0 when typeof(T) == typeof(int) => je.GetInt32(),
                0 when typeof(T) == typeof(uint) => je.GetUInt32(),
                0 when typeof(T) == typeof(long) => je.GetInt64(),
                0 when typeof(T) == typeof(ulong) => je.GetUInt64(),
                0 when typeof(T) == typeof(float) => je.GetSingle(),
                0 when typeof(T) == typeof(double) => je.GetDouble(),
                0 when typeof(T) == typeof(bool) => je.GetBoolean(),
                0 when typeof(T) == typeof(Rect) => je.GetRect(),
                0 when typeof(T) == typeof(PaddingType) => je.GetPadding(),
                0 when typeof(T).IsEnum => (dynamic)je.GetUInt16(),
                _ => throw new NotSupportedException()
            }));

    public JsonObject ToJson(string callerName)
    {
        if (Binding.HasValue)
            return new()
            {
                ["name"] = callerName,
                ["type"] = nameof(Binding),
                ["value"] = Binding.Value is -1 ? "*" : $"${Binding.Value}"
            };
        switch (0)
        {
            case 0 when typeof(T).IsPrimitive:
            case 0 when typeof(T).IsEnum:
                return new()
                {
                    ["name"] = callerName,
                    ["type"] = typeof(T).Name,
                    ["value"] = JsonValue.Create(_value)
                };
            case 0 when typeof(T) == typeof(Rect):
                var r = (_value as Rect?)!.Value;
                return new()
                {
                    ["name"] = callerName,
                    ["type"] = nameof(Rect),
                    ["value"] = new JsonArray(r.Item1, r.Item2)
                };
            case 0 when typeof(T) == typeof(PaddingType):
                var p = (_value as PaddingType?)!.Value;
                return new()
                {
                    ["name"] = callerName,
                    ["type"] = nameof(PaddingType),
                    ["value"] = p.ToJson()
                }; 
            default:
                throw new NotSupportedException();

        }
    }
}
