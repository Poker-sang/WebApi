using System.Text.Json;
using System.Text.Json.Nodes;
using WebApi.TorchUtilities.Services;

namespace WebApi.TorchUtilities.Misc;

public static class Optional
{
    public static Optional<dynamic> FromJson(Type t, JsonElement je) =>
        je.TrySplitOptParam(out var rst)
            ? new() { Binding = rst }
            : new(0 switch
            {
                0 when t == typeof(sbyte) => je.GetSByte(),
                0 when t == typeof(byte) => je.GetByte(),
                0 when t == typeof(short) => je.GetInt16(),
                0 when t == typeof(ushort) => je.GetUInt16(),
                0 when t == typeof(int) => je.GetInt32(),
                0 when t == typeof(uint) => je.GetUInt32(),
                0 when t == typeof(long) => je.GetInt64(),
                0 when t == typeof(ulong) => je.GetUInt64(),
                0 when t == typeof(float) => je.GetSingle(),
                0 when t == typeof(double) => je.GetDouble(),
                0 when t == typeof(bool) => je.GetBoolean(),
                0 when t == typeof(Rect) => je.GetRect(),
                0 when t == typeof(PaddingType) => je.GetPadding(),
                0 when t.IsEnum => je.GetUInt16(),
                _ => throw new NotSupportedException()
            });

    public static Optional<dynamic> FromJson(Type t, object o) =>
            new(0 switch
            {
                0 when t == typeof(sbyte) => (sbyte)o,
                0 when t == typeof(byte) => (byte)o,
                0 when t == typeof(short) => (short)o,
                0 when t == typeof(ushort) => (ushort)o,
                0 when t == typeof(int) => (int)o,
                0 when t == typeof(uint) => (uint)o,
                0 when t == typeof(long) => (long)o,
                0 when t == typeof(ulong) => (ulong)o,
                0 when t == typeof(float) => (float)o,
                0 when t == typeof(double) => (double)o,
                0 when t == typeof(bool) => (bool)o,
                0 when t == typeof(Rect) => (Rect)o,
                0 when t == typeof(PaddingType) => (PaddingType)o,
                0 when t.IsEnum => (ushort)o,
                _ => throw new NotSupportedException()
            });

    public static JsonObject ToJson(this Optional<object> o, Type t, string callerName)
    {
        var isBinding = false;
        var bindingValue = (JsonNode?)null;
        if (o.Binding.HasValue)
        {
            isBinding = true;
            bindingValue = o.Binding.Value;
        }

        switch (0)
        {
            case 0 when t.IsPrimitive:
            case 0 when t.IsEnum:
                return new()
                {
                    ["name"] = callerName,
                    ["type"] = t.Name,
                    ["isBinding"] = isBinding,
                    ["value"] = bindingValue ?? JsonValue.Create(o.TryGetValue)
                };
            case 0 when t == typeof(Rect):
                var r = (o.TryGetValue as Rect?)!.Value;
                return new()
                {
                    ["name"] = callerName,
                    ["type"] = nameof(Rect),
                    ["isBinding"] = isBinding,
                    ["value"] = bindingValue ?? new JsonArray(r.Item1, r.Item2)
                };
            case 0 when t == typeof(PaddingType):
                var p = (o.TryGetValue as PaddingType?)!.Value;
                return new()
                {
                    ["name"] = callerName,
                    ["type"] = nameof(PaddingType),
                    ["isBinding"] = isBinding,
                    ["value"] = bindingValue ?? p.ToJson()
                };
            default:
                throw new NotSupportedException();
        }
    }
}

public struct Optional<T>
{
    private readonly T? _value;
    public int? Binding { get; set; }

    private void RestrictGenerics()
    {
        if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Optional<>))
            throw new InvalidDataException($"Invalid Generic {typeof(T)}");
    }

    public Optional() => RestrictGenerics();

    public Optional(T? value)
    {
        RestrictGenerics();
        _value = value;
    }

    public static implicit operator Optional<object>(Optional<T> v) => new(v._value) { Binding = v.Binding };
    public static implicit operator Optional<T>(Optional<object> v) => new((T?)v._value) { Binding = v.Binding };
    public static implicit operator Optional<T>(T v) => new(v);
    public static implicit operator T(Optional<T> v) => v._value ?? throw new InvalidDataException();

    public T? TryGetValue => _value ?? default;

    public static Optional<T> FromJson(JsonElement je) => Optional.FromJson(typeof(T), je);

    public JsonObject ToJson(string callerName) => Optional.ToJson(this, typeof(T), callerName);
}
