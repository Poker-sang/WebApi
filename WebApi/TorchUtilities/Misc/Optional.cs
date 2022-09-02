using System.Text.Json;
using System.Text.Json.Nodes;
using WebApi.TorchUtilities.Services;

namespace WebApi.TorchUtilities.Misc;

public static class Optional
{
    public static Optional<dynamic> FromJson(Type t, JsonNode jn) =>
        jn.TrySplitOptParam(out var rst)
            ? new() { Binding = rst }
            : new(0 switch
            {
                0 when t == typeof(sbyte) => jn.GetValue<sbyte>(),
                0 when t == typeof(byte) => jn.GetValue<byte>(),
                0 when t == typeof(short) => jn.GetValue<short>(),
                0 when t == typeof(ushort) => jn.GetValue<ushort>(),
                0 when t == typeof(int) => jn.GetValue<int>(),
                0 when t == typeof(uint) => jn.GetValue<uint>(),
                0 when t == typeof(long) => jn.GetValue<long>(),
                0 when t == typeof(ulong) => jn.GetValue<ulong>(),
                0 when t == typeof(float) => jn.GetValue<float>(),
                0 when t == typeof(double) => jn.GetValue<double>(),
                0 when t == typeof(bool) => jn.GetValue<bool>(),
                0 when t == typeof(Rect) => jn.GetRect(),
                0 when t == typeof(PaddingType) => jn.GetPadding(),
                0 when t.IsEnum => jn.GetValue<ushort>(),
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

public struct Optional<T> where T : notnull
{
    private readonly T? _value;
    /// <summary>
    /// 取值：
    /// <br/>[0, +inf)表示绑定第n个参数
    /// <br/>-1("*")表示绑定默认参数：<see langword="long"/>InputChannels
    /// <br/>-2表示缺少参数
    /// </summary>
    public int? Binding { get; set; }

    private static void RestrictGenerics()
    {
        if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Optional<>))
            throw new InvalidDataException($"Invalid Generic {typeof(T)}");
    }

    public static Optional<T> Default => new() { Binding = -2 };

    public Optional() => RestrictGenerics();

    public Optional(T? value)
    {
        RestrictGenerics();
        _value = value;
    }

    public static implicit operator Optional<object>(Optional<T> v) => new(v._value) { Binding = v.Binding };
    public static implicit operator Optional<T>(Optional<object> v) => v._value is null ? new() { Binding = v.Binding } : new((T?)v._value);

    public static implicit operator Optional<T>(T v) => new(v);
    public static implicit operator T(Optional<T> v) => v._value ?? throw new NullReferenceException();

    public T? TryGetValue => _value ?? default;

    public static Optional<T> FromJson(JsonNode jn) => Optional.FromJson(typeof(T), jn);

    public JsonObject ToJson(string callerName) => Optional.ToJson(this, typeof(T), callerName);
}
