using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using WebApi.TorchUtilities.Services;

namespace WebApi.TorchUtilities.Misc;

public class OptionalDynamic : OptionalBase
{
    /// <inheritdoc cref="Optional{T}.Default"/>
    public static OptionalDynamic Default(Type type) => FromBinding(-2, type);

    /// <inheritdoc cref="Optional{T}._value"/>
    private object? _value;

    /// <inheritdoc cref="Optional{T}._binding"/>
    private int? _binding;

    /// <summary>
    /// <see cref="_value"/>的类型
    /// </summary>
    public override Type Type { get; }

    /// <inheritdoc cref="Optional{T}.ValueType"/>
    [MemberNotNullWhen(true, nameof(_value))]
    [MemberNotNullWhen(false, nameof(_binding))]
    public bool ValueType { get; set; }

    /*
    public bool Changed { get; private set; }

    public void TrySet(OptionalDynamic o)
    {
        if (Equals(o))
            return;
        ValueType = o.ValueType;
        if (o.ValueType)
            _value = o._value;
        else
            _binding = o._binding;
        RestrictGenerics();
        Changed = true;
    }
    */

    public object Value => ValueType ? _value : throw new NullReferenceException();
    // public object? TryGetValue() => _value;
    public int Binding => !ValueType ? _binding.Value : throw new NullReferenceException();
    public int? TryGetBinding() => _binding;

    private OptionalDynamic(Type type)
    {
        Type = type;
        RestrictGenerics();
    }

    public static OptionalDynamic FromValue(object value) => new(value.GetType()) { _value = value, ValueType = true };
    public static OptionalDynamic FromBinding(int? binding, Type type) => new(type) { _binding = binding, ValueType = false };

    /// <summary>
    /// Json转指定类型的<see cref="OptionalDynamic"/>
    /// </summary>
    /// <param name="t">值类型</param>
    /// <param name="jn">Json</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"/>
    public static OptionalDynamic FromJson(Type t, JsonNode jn) =>
        jn.TrySplitOptParam(out var rst)
            ? FromBinding(rst, t)
            : FromValue(0 switch
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

    public JsonObject ToWebJson(Type t, string callerName)
    {
        var isBinding = false;
        var bindingValue = (JsonNode?)null;
        if (TryGetBinding() is { } i)
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
                    ["value"] = bindingValue ?? JsonValue.Create(Value)
                };
            case 0 when t == typeof(Rect):
                return new()
                {
                    ["name"] = callerName,
                    ["type"] = nameof(Rect),
                    ["isBinding"] = isBinding,
                    ["value"] = bindingValue ?? new JsonArray(Value.Cast<Rect>().Item1, Value.Cast<Rect>().Item2)
                };
            case 0 when t == typeof(PaddingType):
                return new()
                {
                    ["name"] = callerName,
                    ["type"] = nameof(PaddingType),
                    ["isBinding"] = isBinding,
                    ["value"] = bindingValue ?? Value.Cast<PaddingType>().ToJson()
                };
            default:
                throw new NotSupportedException($"{t.FullName} Not Supported");
        }
    }

    /*
    public JsonNode ToSqlJson(Type t)
    {
        if (TryGetBinding() is { } i)
        {
            if (i < -1)
                throw new InvalidDataException();
            return JsonValue.Create(i is -1 ? "*" : $"${i}")!;
        }

        switch (0)
        {
            case 0 when t.IsPrimitive:
            case 0 when t.IsEnum:
                return JsonValue.Create(Value)!;
            case 0 when t == typeof(Rect):
                return new JsonArray(Value.Cast<Rect>().Item1, Value.Cast<Rect>().Item2);
            case 0 when t == typeof(PaddingType):
                return Value.Cast<PaddingType>().ToJson();
            default:
                throw new NotSupportedException();
        }
    }
    public override bool Equals(object? obj) => obj is not null && Equals(obj);

    public bool Equals(OptionalDynamic? other) =>
        other is not null
        && ValueType == other.ValueType
        && (ValueType
            ? EqualityComparer<object>.Default.Equals(_value, other._value)
            : _binding == other._binding);

    public override int GetHashCode() => base.GetHashCode();
    */
}
