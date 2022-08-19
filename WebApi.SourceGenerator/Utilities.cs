using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Text;

namespace WebApi.SourceGenerator;

internal static class Utilities
{
    internal static INamedTypeSymbol ObjectSymbol { get; set; } = null!;

    /// <summary>
    /// 缩进（伪tab）
    /// </summary>
    /// <param name="n">tab数量</param>
    /// <returns>4n个space</returns>
    internal static string Spacing(int n)
    {
        var temp = "";
        for (var i = 0; i < n; i++)
            temp += "    ";
        return temp;
    }

    /// <summary>
    /// 获取某type的namespace并加入namespaces集合
    /// </summary>
    /// <param name="namespaces">namespaces集合</param>
    /// <param name="usedTypes">已判断过的types</param>
    /// <param name="contextType">上下文所在的类</param>
    /// <param name="symbol">type的symbol</param>
    internal static void UseNamespace(this HashSet<string> namespaces, HashSet<ITypeSymbol> usedTypes, INamedTypeSymbol contextType, ITypeSymbol symbol)
    {
        if (usedTypes.Contains(symbol))
            return;

        _ = usedTypes.Add(symbol);

        var ns = symbol.ContainingNamespace;
        if (!SymbolEqualityComparer.Default.Equals(ns, contextType.ContainingNamespace))
            _ = namespaces.Add(ns.ToDisplayString());

        if (symbol is INamedTypeSymbol { IsGenericType: true } genericSymbol)
            foreach (var a in genericSymbol.TypeArguments)
                UseNamespace(namespaces, usedTypes, contextType, a);
    }

    /// <summary>
    /// 生成nullable预处理语句和引用命名空间
    /// <br/>#nullable enable
    /// <br/>using ...;
    /// <br/>using ...;
    /// <br/>...
    /// </summary>
    /// <param name="namespaces">namespaces集合</param>
    internal static StringBuilder GenerateFileHeader(this HashSet<string> namespaces)
    {
        var stringBuilder = new StringBuilder().AppendLine("#nullable enable\n");
        foreach (var s in namespaces)
            _ = stringBuilder.AppendLine($"using {s};");
        return stringBuilder;
    }
}
