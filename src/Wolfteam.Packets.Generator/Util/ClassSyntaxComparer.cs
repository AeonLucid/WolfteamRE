// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-27.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Wolfteam.Packets.Generator.Util;

public class ClassSyntaxComparer : IEqualityComparer<(ClassDeclarationSyntax, Compilation)>
{
    public static readonly ClassSyntaxComparer Instance = new ClassSyntaxComparer();

    public bool Equals((ClassDeclarationSyntax, Compilation) x, (ClassDeclarationSyntax, Compilation) y)
    {
        return x.Item1.Equals(y.Item1);
    }

    public int GetHashCode((ClassDeclarationSyntax, Compilation) obj)
    {
        return obj.Item1.GetHashCode();
    }
}