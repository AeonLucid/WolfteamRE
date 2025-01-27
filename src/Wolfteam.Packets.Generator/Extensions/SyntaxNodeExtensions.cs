// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-27.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Wolfteam.Packets.Generator.Extensions;

internal static class SyntaxNodeExtensions
{
    public static bool ImplementsPacketPayloadType(this SyntaxNode node, SemanticModel semanticModel)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(node);
        var symbol = symbolInfo.Symbol;
        if (symbol == null)
        {
            return false;
        }

        foreach (var syntaxReference in symbol.DeclaringSyntaxReferences)
        {
            var syntax = syntaxReference.GetSyntax();
            var syntaxSemanticModel = semanticModel.Compilation.GetSemanticModel(syntax.SyntaxTree);
            if (syntax is ClassDeclarationSyntax syntaxClass &&
                syntaxClass.ImplementsPacketPayloadType(syntaxSemanticModel))
            {
                return true;
            }
        }

        return false;
    }
}