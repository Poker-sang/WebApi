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

    public JsonObject ToJson() =>
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
            _ = deserializeEndAndToJsonBegin.AppendLine(PropertyToJson(property, declaration));
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
        var isPrimitive = true;
        if (PresetTypes.TryGetValue(type, out var value))
            type = value;
        else if (type is "bool")
            type = nameof(Boolean);
        else
            isPrimitive = false;

        return $"{Spacing(4)}case nameof({name}): result.{name} = {(isPrimitive
            ? @$"jp.Value.Get{type}"
            : @$"({type})jp.Value.GetUInt16")}(); break;";
    }

    private static string PropertyToJson(IPropertySymbol property, PropertyDeclarationSyntax declaration)
    {
        var init = declaration.Initializer?.Value.ToString();
        var type = declaration.Type.ToString();
        if (type.StartsWith(nameof(Nullable)))
            type = type.Substring(9, type.Length - 1);
        if (init is null)
            if (property.Type.TypeKind is TypeKind.Struct or TypeKind.Enum)
                if (property.NullableAnnotation is NullableAnnotation.Annotated)
                    init = "null";
                else
                    init = PresetTypes.ContainsKey(type) ? "0" : property.Type.Name is nameof(Boolean) ? "false" : $"({type})0";
            else
                throw new NotSupportedException("It is not a struct");
        else if (property.Type.TypeKind is TypeKind.Enum)
            init = $"(int){init}";
        return $@"{Spacing(3)}[nameof({property.Name})] = {init},";
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
        ["Rect"] = "Rect"
    };
}
