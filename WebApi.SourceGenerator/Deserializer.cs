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
    public static {name} Deserialize(JsonElement jsonElement)
    {{
        var result = new {name}();
        foreach (var jp in jsonElement.EnumerateObject())
            switch (jp.Name)
            {{
");
        var deserializeEndAndToJsonBegin = new StringBuilder(@$"{Spacing(3)}}}
        return result;
    }}

    public JsonArray ToJson() =>
        new()
        {{
");
        const string toJsonEndAndClassEnd = $@"
        }};
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
            _ = deserializeEndAndToJsonBegin.AppendLine(PropertyToJson(property));
        }

        deserializeEndAndToJsonBegin = deserializeEndAndToJsonBegin.Remove(deserializeEndAndToJsonBegin.Length - 3, 3);
        return namespaces.GenerateFileHeader()
            .Append(classBeginAndDeserializeBegin)
            .Append(deserializeEndAndToJsonBegin)
            .AppendLine(toJsonEndAndClassEnd)
            .ToString();
    }

    private static string PropertyDeserialize(IPropertySymbol property, PropertyDeclarationSyntax declaration)
    {
        var name = property.Name;
        var type = declaration.Type.ToString();
        if (type.EndsWith("?"))
            type = type.Substring(0, type.Length - 1);

        return $"{Spacing(4)}case nameof({name}): result.{name} = {type}.FromJson(jp.Value); break;";
    }

    private static readonly Dictionary<string, string> PresetTypes = new()
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

    private static string PropertyToJson(IPropertySymbol property)
        => $@"{Spacing(3)}{property.Name}.ToJson(nameof({property.Name})),";

}
