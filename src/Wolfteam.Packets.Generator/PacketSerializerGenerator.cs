// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-27.

using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Wolfteam.Packets.Attributes;
using Wolfteam.Packets.Generator.CodeGen;
using Wolfteam.Packets.Generator.Extensions;
using Wolfteam.Packets.Generator.Util;

namespace Wolfteam.Packets.Generator;

[Generator(LanguageNames.CSharp)]
public class PacketSerializerGenerator : IIncrementalGenerator
{
    public PacketSerializerGenerator()
    {
// #if DEBUG 
//         if (!Debugger.IsAttached) 
//         { 
//             Debugger.Launch(); 
//         }
// #endif
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        var source = classDeclarations
            .Combine(context.CompilationProvider)!
            .WithComparer(ClassSyntaxComparer.Instance);
        
        context.RegisterSourceOutput(source, static (context, source) =>
        {
            var (classDeclaration, compilation) = source;

            Generate(context, compilation, classDeclaration);
        });
    }

    // Quick filter to find the syntax nodes we are interested in.
    private static bool IsSyntaxTargetForGeneration(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not ClassDeclarationSyntax classDeclarationSyntax)
        {
            return false;
        }
        
        if (classDeclarationSyntax.BaseList == null)
        {
            return false;
        }
        
        if (!classDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return false;
        }

        return true;
    }

    // More detailed analysis of the syntax nodes found in the predicate.
    private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classDeclarationSyntax)
        {
            return null;
        }

        if (classDeclarationSyntax.BaseList == null)
        {
            return null;
        }
        
        if (!classDeclarationSyntax.BaseList.Types.Any(x => x.Type.IsPacketPayloadType(context.SemanticModel)))
        {
            return null;
        }
        
        return classDeclarationSyntax;
    }

    public static void Generate(SourceProductionContext context, Compilation compilation, ClassDeclarationSyntax candidate)
    {
        var semanticModel = compilation.GetSemanticModel(candidate.SyntaxTree);
        
        var classNamespace = candidate.GetNamespace();
        var className = candidate.GetName();

        var sourceHint = $"{classNamespace}_{className}_Serializer";
        var sourceText = BuildFile(new BuildContext(context, semanticModel, candidate));

        context.AddSource(sourceHint, SourceText.From(sourceText, Encoding.UTF8));
    }

    private static string BuildFile(BuildContext build)
    {
        var ident = "";
        var builder = new StringBuilder();

        var classNamespace = build.Syntax.GetNamespace();
        var className = build.Syntax.GetName();

        builder.AppendFormat("{0}using System;\n", ident);
        builder.AppendFormat("{0}using System.Text;\n", ident);
        builder.AppendFormat("{0}using System.Buffers;\n", ident);
        builder.AppendFormat("{0}using System.Buffers.Binary;\n", ident);
        builder.AppendFormat("#pragma warning disable 219\n");
        builder.AppendFormat("\n");

        builder.AppendFormat("{0}namespace {1}\n", ident, classNamespace);
        builder.AppendFormat("{0}{{\n", ident);
        ident += Constants.DefaultIdent;

        builder.AppendFormat("{0}public partial class {1}\n", ident, className);
        builder.AppendFormat("{0}{{\n", ident);
        ident += Constants.DefaultIdent;

        // Size
        builder.AppendFormat("{0}public int Size(global::Wolfteam.Packets.ClientVersion version)\n", ident);
        builder.AppendFormat("{0}{{\n", ident);
        BuildMethod(build, builder, ident + Constants.DefaultIdent, new CodeGenSize());
        builder.AppendFormat("{0}}}\n", ident); // Size
        builder.AppendFormat("\n");

        // Serialize
        builder.AppendFormat("{0}public void Serialize(global::Wolfteam.Packets.ClientVersion version, ref global::Wolfteam.Packets.SpanWriter writer)\n", ident);
        builder.AppendFormat("{0}{{\n", ident);
        BuildMethod(build, builder, ident + Constants.DefaultIdent, new CodeGenSerialize());
        builder.AppendFormat("{0}}}\n", ident); // Serialize
        builder.AppendFormat("\n");

        // Parse
        builder.AppendFormat("{0}public bool Deserialize(global::Wolfteam.Packets.ClientVersion version, ref global::Wolfteam.Packets.SpanReader reader)\n", ident);
        builder.AppendFormat("{0}{{\n", ident);
        BuildMethod(build, builder, ident + Constants.DefaultIdent, new CodeGenDeserialize());
        builder.AppendFormat("{0}}}\n", ident); // Deserialize

        ident = Constants.DefaultIdent;
        builder.AppendFormat("{0}}}\n", ident); // class
        ident = "";
        builder.AppendFormat("{0}}}\n", ident); // namespace

        return builder.ToString();
    }

    private static void BuildMethod(BuildContext build, StringBuilder builder, string ident, ICodeGen codeGen)
    {
        var isSize = codeGen is CodeGenSize;
        var isWrite = codeGen is CodeGenSerialize;
        var isParse = codeGen is CodeGenDeserialize;

        // Prologue.
        if (isSize)
        {
            builder.AppendFormat("{0}var size = 0;\n", ident);
        }
        else if (isWrite)
        {
            builder.AppendFormat("{0}int {1} = 0;\n", ident, CodeGenSerialize.StringSizeRef);
        }
        else if (isParse)
        {
            builder.AppendFormat("{0}uint {1} = 0;\n", ident, CodeGenDeserialize.IntValue);
            builder.AppendFormat("{0}ushort {1} = 0;\n", ident, CodeGenDeserialize.ShortValue);
            builder.AppendFormat("{0}byte {1} = 0;\n", ident, CodeGenDeserialize.ByteValue);
        }

        // Body.
        foreach (var property in build.Syntax.GetProperties())
        {
            var propType = property.Type;
            if (propType is NullableTypeSyntax nullableTypeSyntax)
            {
                propType = nullableTypeSyntax.ElementType;
            }
                
            var propName = property.GetName();
            var propAttribute = property.GetAttribute(build.SemanticModel);

            builder.AppendFormat("{0}// {1}\n", ident, propName);

            // Version check
            try
            {
                if (propAttribute.Version != 0)
                {
                    builder.AppendFormat("{0}if (((int)version & {1}) != 0)\n", ident, (int)propAttribute.Version);
                    builder.AppendFormat("{0}{{\n", ident);
                    WriteProperty(build, codeGen, builder, ident + Constants.DefaultIdent, propType, propName, propAttribute);
                    builder.AppendFormat("{0}}}\n", ident);
                }
                else
                {
                    WriteProperty(build, codeGen, builder, ident, propType, propName, propAttribute);
                }
            }
            catch (Exception e)
            {
                build.Context.ReportDiagnostic(DiagnosticUtil.ExceptionCaught(property.GetLocation(), e));
            }
        }

        // Epilogue
        if (isSize)
        {
            builder.AppendFormat("{0}return size;\n", ident);
        }
        else if (isParse)
        {
            builder.AppendFormat("{0}return true;\n", ident);
        }
    }

    private static void WriteProperty(
        BuildContext build,
        ICodeGen codeGen,
        StringBuilder builder,
        string ident,
        TypeSyntax propType,
        string? propName,
        WolfteamFieldAttribute propAttribute)
    {
        var propTypeName = propType.GetTypeName();
        var propKind = propType.Kind();
        if (propKind == SyntaxKind.PredefinedType)
        {
            switch (propTypeName)
            {
                case "byte":
                    codeGen.WriteU8(builder, ident, propName, propAttribute);
                    break;
                
                case "short":
                case "ushort":
                    codeGen.WriteU16(builder, ident, propName, propAttribute);
                    break;

                case "int":
                case "uint":
                    codeGen.WriteU32(builder, ident, propName, propAttribute);
                    break;

                case "string":
                    if (propAttribute.FixedSize == -1 && propAttribute.LengthSize == 0)
                    {
                        build.Context.ReportDiagnostic(DiagnosticUtil.AttributeFieldMissing(propType.GetLocation(), nameof(propAttribute.LengthSize)));
                        return;
                    }
                    
                    codeGen.WriteString(builder, ident, propName, propAttribute);
                    break;

                default:
                    build.Context.ReportDiagnostic(DiagnosticUtil.UnknownType(propType.GetLocation(), propTypeName));
                    break;
            }
        }
        else if (propKind == SyntaxKind.ArrayType)
        {
            var typeArr = (ArrayTypeSyntax)propType;
            var typeArrElement = typeArr.ElementType;
            var typeGlobal = typeArrElement.GetGlobalTypeName(build.SemanticModel);

            if (propAttribute.FixedSize == -1 && propAttribute.LengthSize == 0)
            {
                build.Context.ReportDiagnostic(DiagnosticUtil.AttributeFieldMissing(propType.GetLocation(), nameof(propAttribute.LengthSize)));
                return;
            }
            
            codeGen.WriteObjectArray(builder, ident, propName, typeGlobal, propAttribute, (elIndent, elRefName) =>
            {
                WriteProperty(build, codeGen, builder, elIndent, typeArrElement, elRefName, propAttribute);
            });
        }
        else if (propKind == SyntaxKind.IdentifierName)
        {
            var typeObj = (IdentifierNameSyntax)propType;
            if (typeObj.ImplementsPacketPayloadType(build.SemanticModel))
            {
                var typeGlobal = typeObj.GetGlobalTypeName(build.SemanticModel);
                
                codeGen.WriteObject(builder, ident, propName, typeGlobal, propAttribute);
            }
            else
            {
                build.Context.ReportDiagnostic(DiagnosticUtil.InvalidObject(propType.GetLocation()));
            }
        }
        else
        {
            build.Context.ReportDiagnostic(DiagnosticUtil.UnknownKind(propType.GetLocation(), propTypeName, propKind));
        }
    }
}