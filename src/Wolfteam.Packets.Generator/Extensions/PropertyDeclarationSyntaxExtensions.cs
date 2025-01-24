using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Generator.Extensions;

internal static class PropertyDeclarationSyntaxExtensions
{
    public static string GetName(this PropertyDeclarationSyntax property)
    {
        return property.Identifier.Text;
    }

    public static string GetTypeName(this TypeSyntax typeSyntax)
    {
        if (typeSyntax is NullableTypeSyntax nullableType)
        {
            return nullableType.ElementType.ToString();
        }
            
        return typeSyntax.ToString();
    }

    public static WolfteamFieldAttribute GetAttribute(this PropertyDeclarationSyntax property, SemanticModel semanticModel)
    {
        if (semanticModel.GetDeclaredSymbol(property) is not IPropertySymbol propertySymbol)
        {
            throw new Exception("Property symbol not found");
        }
            
        var result = new WolfteamFieldAttribute();
            
        foreach (var attributeData in propertySymbol.GetAttributes())
        {
            if (!nameof(WolfteamFieldAttribute).Equals(attributeData.AttributeClass?.Name))
            {
                continue;
            }
                
            foreach (var argument in attributeData.NamedArguments)
            {
                if (argument.Value.IsNull)
                {
                    continue;
                }
                
                switch (argument.Key)
                {
                    case nameof(WolfteamFieldAttribute.LengthSize):
                        result.LengthSize = (int) argument.Value.Value!;
                        break;
                    case nameof(WolfteamFieldAttribute.BigEndian):
                        result.BigEndian = (bool) argument.Value.Value!;
                        break;
                    case nameof(WolfteamFieldAttribute.Encoding):
                        result.Encoding = (FieldEncoding) argument.Value.Value!;
                        break;
                    case nameof(WolfteamFieldAttribute.Version):
                        result.Version = (ClientVersion) argument.Value.Value!;
                        break;
                }
            }
        }

        return result;
    }
}