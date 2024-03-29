﻿using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using WebApi.TorchUtilities.Services;

namespace WebApi.TorchUtilities.Misc;

public class Optional<T> : OptionalBase where T : struct
{
    /// <summary>
    /// 表缺位的常量
    /// </summary>
    public static Optional<T> Default => FromBinding(-2);

    public override Type Type => typeof(T);

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
        {
            _value = o._value;
            _binding = null;
        }
        else
        {
            _value = null;
            _binding = o._binding;
        }
        Changed = true;
    }

    public T Value => ValueType ? _value.Value : throw new NullReferenceException();
    public T? TryGetValue() => _value;
    public int Binding => !ValueType ? _binding.Value : throw new NullReferenceException();
    public int? TryGetBinding() => _binding;

    private Optional() => RestrictGenerics();

    public static Optional<T> FromValue(T? value) => new() { _value = value, ValueType = true };
    public static Optional<T> FromBinding(int? binding) => new() { _binding = binding, ValueType = false };

    /// <summary>
    /// 防止类型转换的时候自动调用构造
    /// </summary>
    public static implicit operator OptionalDynamic(Optional<T> v) => v.ValueType ? OptionalDynamic.FromValue(v._value) : OptionalDynamic.FromBinding(v._binding, typeof(T));
    /// <summary>
    /// 防止类型转换的时候自动调用构造
    /// </summary>
    public static implicit operator Optional<T>(OptionalDynamic v) => v.ValueType ? FromValue((T?)v.Value) : FromBinding(v.Binding);

    public static implicit operator Optional<T>(T v) => FromValue(v);
    public static implicit operator T(Optional<T> v) => v.Value;

    /// <summary>
    /// Json转指定类型的<see cref="Optional{T}"/>
    /// </summary>
    /// <param name="jn">Json</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"/>
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

    public JsonObject ToWebJson(string callerName)
    {
        var t = typeof(T);
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

    public JsonNode ToSqlJson()
    {
        var t = typeof(T);
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

    public bool Equals(Optional<T>? other) =>
        other is not null
        && ValueType == other.ValueType
        && (ValueType
            ? EqualityComparer<T?>.Default.Equals(_value, other._value)
            : _binding == other._binding);

    public override int GetHashCode() => base.GetHashCode();
}
