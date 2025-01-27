// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-27.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Wolfteam.Packets.Generator.Extensions;

namespace Wolfteam.Packets.Generator;

public class PacketSyntaxReceiver : ISyntaxContextReceiver
{
    public List<ClassDeclarationSyntax> Candidates { get; } = new();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classDeclarationSyntax)
        {
            return;
        }

        if (classDeclarationSyntax.BaseList == null)
        {
            return;
        }

        if (!classDeclarationSyntax.BaseList.Types.Any(x => x.Type.IsPacketPayloadType(context.SemanticModel)))
        {
            return;
        }

        if (!classDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return;
        }
        
        Candidates.Add(classDeclarationSyntax);
    }
}