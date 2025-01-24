using System.Diagnostics;
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

[Generator]
public class PacketSerializerGenerator : ISourceGenerator
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

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new PacketSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is PacketSyntaxReceiver packet)
        {
            foreach (var candidate in packet.Candidates)
            {
                var classNamespace = candidate.GetNamespace();
                var className = candidate.GetName();

                var sourceHint = $"{classNamespace}_{className}_Serializer";
                var sourceText = BuildFile(context, candidate);

                context.AddSource(sourceHint, SourceText.From(sourceText, Encoding.UTF8));
            }
        }
    }

    private string BuildFile(GeneratorExecutionContext context, ClassDeclarationSyntax classDeclaration)
    {
        var ident = "";
        var builder = new StringBuilder();

        var classNamespace = classDeclaration.GetNamespace();
        var className = classDeclaration.GetName();

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
        builder.AppendFormat("{0}public int Size(ClientVersion version)\n", ident);
        builder.AppendFormat("{0}{{\n", ident);
        BuildMethod(context, classDeclaration, builder, ident + Constants.DefaultIdent, new CodeGenSize());
        builder.AppendFormat("{0}}}\n", ident); // Size
        builder.AppendFormat("\n");

        // Serialize
        builder.AppendFormat("{0}public void Serialize(ClientVersion version, ref SpanWriter writer)\n", ident);
        builder.AppendFormat("{0}{{\n", ident);
        BuildMethod(context, classDeclaration, builder, ident + Constants.DefaultIdent, new CodeGenSerialize());
        builder.AppendFormat("{0}}}\n", ident); // Serialize
        builder.AppendFormat("\n");

        // Parse
        builder.AppendFormat("{0}public bool Deserialize(ClientVersion version, ref SpanReader reader)\n", ident);
        builder.AppendFormat("{0}{{\n", ident);
        BuildMethod(context, classDeclaration, builder, ident + Constants.DefaultIdent, new CodeGenDeserialize());
        builder.AppendFormat("{0}}}\n", ident); // Deserialize

        ident = Constants.DefaultIdent;
        builder.AppendFormat("{0}}}\n", ident); // class
        ident = "";
        builder.AppendFormat("{0}}}\n", ident); // namespace

        return builder.ToString();
    }

    private static void BuildMethod(GeneratorExecutionContext context, ClassDeclarationSyntax classDeclaration, StringBuilder builder, string ident, ICodeGen codeGen)
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
        var semanticModel = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);
        
        foreach (var property in classDeclaration.GetProperties())
        {
            var propType = property.Type;
            if (propType is NullableTypeSyntax nullableTypeSyntax)
            {
                propType = nullableTypeSyntax.ElementType;
            }
                
            var propName = property.GetName();
            var propAttribute = property.GetAttribute(semanticModel);

            builder.AppendFormat("{0}// {1}\n", ident, propName);

            // Version check
            if (propAttribute.Version != 0)
            {
                builder.AppendFormat("{0}if (((int)version & {1}) != 0)\n", ident, (int) propAttribute.Version);
                builder.AppendFormat("{0}{{\n", ident);
                WriteProperty(context, codeGen, semanticModel, builder, ident + Constants.DefaultIdent, propType, propName, propAttribute);
                builder.AppendFormat("{0}}}\n", ident);
            }
            else
            {
                WriteProperty(context, codeGen, semanticModel, builder, ident, propType, propName, propAttribute);
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
        GeneratorExecutionContext context,
        ICodeGen codeGen,
        SemanticModel semanticModel,
        StringBuilder builder,
        string ident,
        TypeSyntax propType,
        string propName,
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
                    if (propAttribute.LengthSize == 0)
                    {
                        context.ReportDiagnostic(DiagnosticUtil.AttributeFieldMissing(propType.GetLocation(), nameof(propAttribute.LengthSize)));
                        return;
                    }
                    
                    codeGen.WriteString(builder, ident, propName, propAttribute);
                    break;

                default:
                    context.ReportDiagnostic(DiagnosticUtil.UnknownType(propType.GetLocation(), propTypeName));
                    break;
            }
        }
        else if (propKind == SyntaxKind.ArrayType)
        {
            var typeArr = (ArrayTypeSyntax)propType;
            var typeArrElement = typeArr.ElementType;
            var typeGlobal = typeArrElement.GetGlobalTypeName(semanticModel);

            if (propAttribute.LengthSize == 0)
            {
                context.ReportDiagnostic(DiagnosticUtil.AttributeFieldMissing(propType.GetLocation(), nameof(propAttribute.LengthSize)));
                return;
            }
            
            codeGen.WriteObjectArray(builder, ident, propName, typeGlobal, propAttribute, (elIndent, elRefName) =>
            {
                WriteProperty(context, codeGen, semanticModel, builder, elIndent, typeArrElement, elRefName, propAttribute);
            });
        }
        else if (propKind == SyntaxKind.IdentifierName)
        {
            var typeObj = (IdentifierNameSyntax)propType;
            if (typeObj.ImplementsPacketPayloadType(semanticModel))
            {
                var typeGlobal = typeObj.GetGlobalTypeName(semanticModel);
                
                codeGen.WriteObject(builder, ident, propName, typeGlobal, propAttribute);
            }
            else
            {
                context.ReportDiagnostic(DiagnosticUtil.InvalidObject(propType.GetLocation()));
            }
        }
        else
        {
            context.ReportDiagnostic(DiagnosticUtil.UnknownKind(propType.GetLocation(), propTypeName, propKind));
        }
    }
}