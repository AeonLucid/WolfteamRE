using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Wolfteam.Packets.Generator.Extensions;

internal static class ClassDeclarationSyntaxExtensions
{
    public static string GetName(this ClassDeclarationSyntax classDeclaration)
    {
        return classDeclaration.Identifier.Text;
    }

    public static string GetNamespace(this ClassDeclarationSyntax classDeclaration)
    {
        if (classDeclaration.Parent is NamespaceDeclarationSyntax namespaceDeclaration)
        {
            return namespaceDeclaration.Name.ToString();
        }
            
        if (classDeclaration.Parent is FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclarationSyntax)
        {
            return fileScopedNamespaceDeclarationSyntax.Name.ToString();
        }
            
        throw new Exception("Could not find namespace");
    }

    public static IEnumerable<PropertyDeclarationSyntax> GetProperties(this ClassDeclarationSyntax classDeclaration)
    {
        return classDeclaration.ChildNodes().OfType<PropertyDeclarationSyntax>();
    }

    public static bool ImplementsPacketPayloadType(this ClassDeclarationSyntax node, SemanticModel semanticModel)
    {
        if (node.BaseList == null)
        {
            return false;
        }

        return node.BaseList.Types.Any(x => x.Type.IsPacketPayloadType(semanticModel));
    }
}