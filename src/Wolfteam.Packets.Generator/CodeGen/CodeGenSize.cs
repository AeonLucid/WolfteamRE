using System;
using System.Text;
using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Generator.CodeGen;

internal class CodeGenSize : ICodeGen
{
    public void WriteU8(StringBuilder builder, string ident, object value, WolfteamFieldAttribute attribute)
    {
        builder.AppendFormat("{0}size += {1};\n", ident, sizeof(byte));
    }

    public void WriteU16(StringBuilder builder, string ident, object value, WolfteamFieldAttribute attribute)
    {
        builder.AppendFormat("{0}size += {1};\n", ident, sizeof(ushort));
    }

    public void WriteU32(StringBuilder builder, string ident, object value, WolfteamFieldAttribute attribute)
    {
        builder.AppendFormat("{0}size += {1};\n", ident, sizeof(uint));
    }

    public void WriteString(StringBuilder builder, string ident, string refName, WolfteamFieldAttribute attribute)
    {
        switch (attribute.LengthSize)
        {
            case 1:
                WriteU8(builder, ident, 0, attribute);
                break;
            case 2:
                WriteU16(builder, ident, 0, attribute);
                break;
            case 4:
                WriteU32(builder, ident, 0, attribute);
                break;
            default:
                throw new NotImplementedException($"Length {attribute.LengthSize} not implemented");
        }

        builder.AppendFormat("{0}if (!string.IsNullOrEmpty({1}))\n", ident, refName);
        builder.AppendFormat("{0}{{\n", ident);
        builder.AppendFormat("{0}size += Encoding.{1}.GetByteCount({2});\n", ident + Constants.DefaultIdent, attribute.Encoding, refName);
        builder.AppendFormat("{0}}}\n", ident);
    }

    public void WriteObject(StringBuilder builder, string ident, string refName, string typeName, WolfteamFieldAttribute attribute)
    {
        builder.AppendFormat("{0}size += {1}.Size(version);\n", ident, refName);
    }

    public void WriteObjectArray(StringBuilder builder, string ident, string refName, string typeName, WolfteamFieldAttribute attribute, ArrayElementDelegate writeElement)
    {
        switch (attribute.LengthSize)
        {
            case 1:
                WriteU8(builder, ident, 0, attribute);
                break;
            case 2:
                WriteU16(builder, ident, 0, attribute);
                break;
            case 4:
                WriteU32(builder, ident, 0, attribute);
                break;
            default:
                throw new NotImplementedException($"Length {attribute.LengthSize} not implemented");
        }
            
        builder.AppendFormat("{0}if ({1} != null)\n", ident, refName);
        builder.AppendFormat("{0}{{\n", ident);
        builder.AppendFormat("{0}for (var i = 0; i < {1}.Length; i++)\n", ident + Constants.DefaultIdent, refName);
        builder.AppendFormat("{0}{{\n", ident + Constants.DefaultIdent);
        writeElement(ident + Constants.DefaultIdent + Constants.DefaultIdent, $"{refName}[i]");
        builder.AppendFormat("{0}}}\n", ident + Constants.DefaultIdent);
        builder.AppendFormat("{0}}}\n", ident);
    }
}