// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-27.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Wolfteam.Packets.Generator.Extensions;

internal static class TypeSyntaxExtensions
{
    public static bool IsPacketPayloadType(this TypeSyntax node, SemanticModel semanticModel)
    {
        if (node.ToString() != Constants.PacketPayloadName)
        {
            return false;
        }
            
        var symbolInfo = semanticModel.GetSymbolInfo(node);
        var symbol = symbolInfo.Symbol;
        if (symbol == null)
        {
            return false;
        }

        var typeNamespace = symbol.ContainingNamespace.ToString();
        if (typeNamespace != Constants.PacketPayloadNamespace)
        {
            return false;
        }

        return true;
    }
    
    public static string GetGlobalTypeName(this TypeSyntax node, SemanticModel semanticModel)
    {
        var semType = semanticModel.GetTypeInfo(node);
        var typeNamespace = semType.Type?.ContainingNamespace.ToString();
        var typeClass = semType.Type?.Name;
        var typeGlobal = $"global::{typeNamespace}.{typeClass}";

        return typeGlobal;
    }
}