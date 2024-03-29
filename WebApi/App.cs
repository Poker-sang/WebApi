﻿using System.Runtime.CompilerServices;

namespace WebApi;

public static class App
{
    private static readonly DateTime _unixStart = new(1970, 1, 1, 8, 0, 0);
    public static long Now => (long)(DateTime.Now - _unixStart).TotalMilliseconds;
    public static long ToUnixTimeStamp(this DateTime d) => (long)(d - _unixStart).TotalMilliseconds;

    public static string CamelName(this object? o, [CallerArgumentExpression("o")] string name = "") => name.ToCamel();

    public static string ToCamel(this string name)
    {
        var index = name.LastIndexOf('.') + 1;
        return name[index..(index + 1)].ToLower() + name[(index + 1)..];
    }
}
