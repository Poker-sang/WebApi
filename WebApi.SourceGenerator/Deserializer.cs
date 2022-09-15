using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static WebApi.SourceGenerator.Utilities;

namespace WebApi.SourceGenerator;

internal static partial class TypeWithAttributeDelegates
{
    public static string? Deserializer(TypeDeclarationSyntax typeDeclaration, INamedTypeSymbol typeSymbol, List<AttributeData> attributeList)
    {
        var name = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        var namespaces = new HashSet<string>
        {
            "System.Text.Json",
            "System.Text.Json.Nodes",
            "WebApi.TorchUtilities.Attributes",
            "WebApi.TorchUtilities.Interfaces",
            "WebApi.TorchUtilities.Services"
        };
        var usedTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        var classBeginAndDeserializeBegin = new StringBuilder(@$"
namespace {typeSymbol.ContainingNamespace.ToDisplayString()};

partial class {name} : IDeserialize<{name}>
{{
    public static {name} Deserialize(JsonObject jsonObject)
    {{
        var result = new {name}();
        foreach (var (key, value) in jsonObject)
            switch (key)
            {{
");
        var deserializeEndAndToFullJsonBegin = new StringBuilder(@$"{Spacing(3)}}}
        return result;
    }}

    public JsonArray ToWebJson() =>
        new()
        {{
");
        var toFullJsonEndAndToJsonBegin = new StringBuilder(@$"
        }};

    public JsonObject ToSqlJson() 
    {{
        var jo = new JsonObject {{ [""Name""] = nameof({name}) }};
");
        const string toJsonEndAndClassEnd = $@"
        return jo;
    }}
}}";

        var dict = typeDeclaration.Members
            .Where(t => t.Kind() is SyntaxKind.PropertyDeclaration)
            .Cast<PropertyDeclarationSyntax>()
            .ToDictionary(d => d.Identifier.ValueText, d => d);
        foreach (var property in typeSymbol.GetMembers().Where(property =>
                         property is { Kind: SymbolKind.Property } and not { Name: "EqualityContract" }
                         && !property.GetAttributes().Any(propertyAttribute =>
                             propertyAttribute.AttributeClass!.Name is "DeserializerIgnoreAttribute"))
                     .Cast<IPropertySymbol>())
        {
            namespaces.UseNamespace(usedTypes, typeSymbol, property.Type);
            var declaration = dict[property.Name];
            _ = classBeginAndDeserializeBegin.AppendLine(PropertyDeserialize(property, declaration));
            _ = deserializeEndAndToFullJsonBegin.AppendLine(PropertyToJsonFull(property));
            _ = toFullJsonEndAndToJsonBegin.AppendLine(PropertyToJson(property));
        }

        deserializeEndAndToFullJsonBegin = deserializeEndAndToFullJsonBegin.Remove(deserializeEndAndToFullJsonBegin.Length - 3, 3);
        toFullJsonEndAndToJsonBegin = toFullJsonEndAndToJsonBegin.Remove(toFullJsonEndAndToJsonBegin.Length - 2, 2);
        return namespaces.GenerateFileHeader()
            .Append(classBeginAndDeserializeBegin)
            .Append(deserializeEndAndToFullJsonBegin)
            .Append(toFullJsonEndAndToJsonBegin)
            .AppendLine(toJsonEndAndClassEnd)
            .ToString();
    }

    private static string PropertyDeserialize(IPropertySymbol property, PropertyDeclarationSyntax declaration)
    {
        var name = property.Name;
        var type = declaration.Type.ToString();
        if (type.EndsWith("?"))
            type = type.Substring(0, type.Length - 1);

        return $"{Spacing(4)}case nameof({name}): result.{name}.TrySet({type}.FromJson(value!)); break;";
    }

    private static readonly Dictionary<string, string> _presetTypes = new()
    {
        ["sbyte"] = nameof(SByte),
        ["byte"] = nameof(Byte),
        ["short"] = nameof(Int16),
        ["ushort"] = nameof(UInt16),
        ["int"] = nameof(Int32),
        ["uint"] = nameof(UInt32),
        ["long"] = nameof(Int64),
        ["ulong"] = nameof(UInt64),
        ["float"] = nameof(Single),
        ["double"] = nameof(Double),
        ["bool"] = nameof(Boolean),
        ["Rect"] = "Rect",
        ["Padding"] = "Padding"
    };

    private static string PropertyToJsonFull(IPropertySymbol property)
        => $"{Spacing(3)}{property.Name}.ToWebJson(nameof({property.Name})),";

    private static string PropertyToJson(IPropertySymbol property)
        => $@"{Spacing(2)}if ({property.Name}.Changed)
            jo.Add(nameof({property.Name}), {property.Name}.ToSqlJson(nameof({property.Name})));";
}
