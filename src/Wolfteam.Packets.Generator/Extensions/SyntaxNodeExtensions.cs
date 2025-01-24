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