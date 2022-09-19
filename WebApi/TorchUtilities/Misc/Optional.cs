using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text.Json.Nodes;
using WebApi.TorchUtilities.Services;

namespace WebApi.TorchUtilities.Misc;

public static class Optional
{
    /// <summary>
    /// Json转指定类型的<see cref="Optional{T}"/>
    /// </summary>
    /// <param name="t">类型</param>
    /// <param name="jn">Json</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"/>
    public static Optional<dynamic> FromJson(Type t, JsonNode jn) =>
        jn.TrySplitOptParam(out var rst)
            ? Optional<dynamic>.FromBinding(rst)
            : Optional<dynamic>.FromValue(0 switch
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
                0 when t == typeof(decimal) => jn.GetValue<decimal>(),
                0 when t == typeof(bool) => jn.GetValue<bool>(),
                0 when t == typeof(Rect) => jn.GetRect(),
                0 when t == typeof(PaddingType) => jn.GetPadding(),
                0 when t.IsEnum => jn.GetValue<int>(),
                _ => throw new NotSupportedException($"{t.FullName} Not Supported")
            });

    public static Optional<dynamic> FromValue(Type t, object o)
        => Optional<dynamic>.FromValue(o switch
        {
            sbyte v => v,
            byte v => v,
            short v => v,
            ushort v => v,
            int v => v,
            uint v => v,
            long v => v,
            ulong v => v,
            float v => v,
            double v => v,
            decimal v => v,
            bool v => v,
            Rect v => v,
            PaddingType v => v,
            Enum => (int)o,
            _ => throw new NotSupportedException()
        });

    public static JsonObject ToWebJson(this Optional<object> o, Type t, string callerName)
    {
        var isBinding = false;
        var bindingValue = (JsonNode?)null;
        if (o.TryGetBinding() is { } i)
        {
            isBinding = true;
            bindingValue = i;
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
                    ["value"] = bindingValue ?? JsonValue.Create(o.GetValue())
                };
            case 0 when t == typeof(Rect):
                return new()
                {
                    ["name"] = callerName,
                    ["type"] = nameof(Rect),
                    ["isBinding"] = isBinding,
                    ["value"] = bindingValue ?? new JsonArray(((Rect)o.GetValue()).Item1, ((Rect)o.GetValue()).Item2)
                };
            case 0 when t == typeof(PaddingType):
                return new()
                {
                    ["name"] = callerName,
                    ["type"] = nameof(PaddingType),
                    ["isBinding"] = isBinding,
                    ["value"] = bindingValue ?? ((PaddingType)o.GetValue()).ToJson()
                };
            default:
                throw new NotSupportedException($"{t.FullName} Not Supported");
        }
    }

    public static JsonNode ToSqlJson(this Optional<object> o, Type t)
    {
        if (o.TryGetBinding() is { } i)
        {
            if (i < -1)
                throw new InvalidDataException();
            return JsonValue.Create(i is -1 ? "*" : $"${i}")!;
        }

        switch (0)
        {
            case 0 when t.IsPrimitive:
            case 0 when t.IsEnum:
                return JsonValue.Create(o.GetValue())!;
            case 0 when t == typeof(Rect):
                return new JsonArray(((Rect)o.GetValue()).Item1, ((Rect)o.GetValue()).Item2);
            case 0 when t == typeof(PaddingType):
                return ((PaddingType)o.GetValue()).ToJson();
            default:
                throw new NotSupportedException();
        }
    }
}

public class Optional<T> where T : notnull
{
    /// <summary>
    /// 表缺位的常量
    /// </summary>
    public static Optional<T> Default => FromBinding(-2);

    /// <summary>
    /// 应该包含的值
    /// </summary>
    private T? _value;

    /// <summary>
    /// 取值：
    /// <br/>[0, +inf)表示绑定第n个参数
    /// <br/>-1("*")表示绑定默认参数：<see langword="long"/> InputChannels
    /// <br/>-2表示缺少参数
    /// </summary>
    private int? _binding;

    /// <summary>
    /// <see langword="true"/>表示有<see cref="_value"/>
    /// <br/><see langword="false"/>表示有<see cref="_binding"/>
    /// </summary>
    [MemberNotNullWhen(true, nameof(_value))]
    [MemberNotNullWhen(false, nameof(_binding))]
    public bool ValueType { get; set; }

    public bool Changed { get; private set; }

    public void TrySet(Optional<T> o)
    {
        if (Equals(o))
            return;
        ValueType = o.ValueType;
        if (o.ValueType)
            _value = o._value;
        else
            _binding = o._binding;
        Changed = true;
    }

    public T GetValue() => ValueType ? _value : throw new NullReferenceException();
    public T? TryGetValue() => _value;
    public int GetBinding() => !ValueType ? _binding.Value : throw new NullReferenceException();
    public int? TryGetBinding() => _binding;

    private static void RestrictGenerics()
    {
        if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Optional<>))
            throw new InvalidDataException($"Invalid Generic {typeof(T).FullName}");
    }

    private Optional() => RestrictGenerics();

    public static Optional<T> FromValue(T? value) => new() { _value = value, ValueType = true };
    public static Optional<T> FromBinding(int? binding) => new() { _binding = binding, ValueType = false };

    /// <summary>
    /// 防止类型转换的时候自动调用构造
    /// </summary>
    public static implicit operator Optional<object>(Optional<T> v) => v.ValueType ? Optional<object>.FromValue(v._value) : Optional<object>.FromBinding(v._binding);
    /// <summary>
    /// 防止类型转换的时候自动调用构造
    /// </summary>
    public static implicit operator Optional<T>(Optional<object> v) => v.ValueType ? FromValue((T?)v._value) : FromBinding(v._binding);

    public static implicit operator Optional<T>(T v) => FromValue(v);
    public static implicit operator T(Optional<T> v) => v.GetValue();

    /// <inheritdoc cref="Optional.FromJson"/>
    public static Optional<T> FromJson(JsonNode jn) =>
        jn.TrySplitOptParam(out var rst)
            ? FromBinding(rst)
            : FromValue(default(T) switch
            {
                sbyte => jn.GetValue<T>(),
                byte => jn.GetValue<T>(),
                short => jn.GetValue<T>(),
                ushort => jn.GetValue<T>(),
                int => jn.GetValue<T>(),
                uint => jn.GetValue<T>(),
                long => jn.GetValue<T>(),
                ulong => jn.GetValue<T>(),
                float => jn.GetValue<T>(),
                double => jn.GetValue<T>(),
                decimal => jn.GetValue<T>(),
                bool => jn.GetValue<T>(),
                Rect => jn.GetRect().Cast<T>(),
                PaddingType => jn.GetPadding().Cast<T>(),
                Enum => jn.GetValue<int>().Cast<T>(),
                _ => throw new NotSupportedException($"{typeof(T).FullName} Not Supported")
            });

    public JsonObject ToWebJson(string callerName) => Optional.ToWebJson(this, typeof(T), callerName);
    public JsonNode ToSqlJson() => Optional.ToSqlJson(this, typeof(T));

    public override bool Equals(object? obj) => obj is not null && Equals(obj);

    public bool Equals(Optional<T>? other) =>
        other is not null
        && ValueType == other.ValueType
        && (ValueType
            ? EqualityComparer<T?>.Default.Equals(_value, other._value)
            : _binding == other._binding);

    public override int GetHashCode() => base.GetHashCode();
}
