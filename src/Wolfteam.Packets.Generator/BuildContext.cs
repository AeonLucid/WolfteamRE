// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-27.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Wolfteam.Packets.Generator;

public class BuildContext
{
    public BuildContext(
        SourceProductionContext context, 
        SemanticModel semanticModel,
        ClassDeclarationSyntax syntax)
    {
        Context = context;
        SemanticModel = semanticModel;
        Syntax = syntax;
    }
    
    public SourceProductionContext Context { get; }
    public SemanticModel SemanticModel { get; }
    public ClassDeclarationSyntax Syntax { get; }
}