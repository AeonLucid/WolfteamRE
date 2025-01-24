using System;
using System.Text;
using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Generator.CodeGen;

internal class CodeGenSerialize : ICodeGen
{
    public const string StringSizeRef = "stringSize";
        
    public void WriteU8(StringBuilder builder, string ident, object value, WolfteamFieldAttribute attribute)
    {
        builder.AppendFormat("{0}writer.WriteU8({1});\n", ident, value);
    }

    public void WriteU16(StringBuilder builder, string ident, object value, WolfteamFieldAttribute attribute)
    {
        builder.AppendFormat(attribute.BigEndian 
                ? "{0}writer.WriteU16BE({1});\n" 
                : "{0}writer.WriteU16({1});\n", ident, value);
    }

    public void WriteU32(StringBuilder builder, string ident, object value, WolfteamFieldAttribute attribute)
    {
        builder.AppendFormat("{0}writer.WriteU32({1});\n", ident, value);
    }

    public void WriteString(StringBuilder builder, string ident, string refName, WolfteamFieldAttribute attribute)
    {
        builder.AppendFormat("{0}if (string.IsNullOrEmpty({1}))\n", ident, refName);
        builder.AppendFormat("{0}{{\n", ident);
        if (attribute.Length == 0)
        {
            switch (attribute.LengthSize)
            {
                case 1:
                    WriteU8(builder, ident + Constants.DefaultIdent, 0, attribute);
                    break;
                case 2:
                    WriteU16(builder, ident + Constants.DefaultIdent, 0, attribute);
                    break;
                case 4:
                    WriteU32(builder, ident + Constants.DefaultIdent, 0, attribute);
                    break;
                default:
                    throw new NotImplementedException($"Length {attribute.LengthSize} not implemented");
            }
        }
        else
        {
            builder.AppendFormat("{0}writer.Skip({1});\n", ident + Constants.DefaultIdent, attribute.Length);
        }
        builder.AppendFormat("{0}}} else {{\n", ident);
        builder.AppendFormat("{0}{1} = Encoding.{2}.GetByteCount({3});\n", ident + Constants.DefaultIdent, StringSizeRef, attribute.Encoding, refName);
        if (attribute.Length == 0)
        {
            switch (attribute.LengthSize)
            {
                case 1:
                    WriteU8(builder, ident + Constants.DefaultIdent, $"Convert.ToByte({StringSizeRef})", attribute);
                    break;
                case 2:
                    WriteU16(builder, ident + Constants.DefaultIdent, $"Convert.ToUInt16({StringSizeRef})", attribute);
                    break;
                case 4:
                    WriteU32(builder, ident + Constants.DefaultIdent, $"Convert.ToUInt32({StringSizeRef})", attribute);
                    break;
                default:
                    throw new NotImplementedException($"Length {attribute.LengthSize} not implemented");
            }
        }
        else
        {
            builder.AppendFormat("{0}if ({1} != {2})\n", ident + Constants.DefaultIdent, StringSizeRef, attribute.Length);
            builder.AppendFormat("{0}{{\n", ident + Constants.DefaultIdent);
            builder.AppendFormat("{0}throw new InvalidOperationException(\"String length mismatch should be {1}\");\n", ident + Constants.DefaultIdent + Constants.DefaultIdent, attribute.LengthSize);
            builder.AppendFormat("{0}}}\n", ident + Constants.DefaultIdent);
        }
        builder.AppendFormat("{0}writer.WriteString(Encoding.{1}, {2});\n", ident + Constants.DefaultIdent, attribute.Encoding, refName);
        builder.AppendFormat("{0}}}\n", ident);
    }

    public void WriteObject(StringBuilder builder, string ident, string refName, string typeName, WolfteamFieldAttribute attribute)
    {
        builder.AppendFormat("{0}{1}.Serialize(version, ref writer);\n", ident, refName);
    }

    public void WriteObjectArray(StringBuilder builder, string ident, string refName, string typeName, WolfteamFieldAttribute attribute, ArrayElementDelegate writeElement)
    {
        builder.AppendFormat("{0}if ({1} == null)\n", ident, refName);
        builder.AppendFormat("{0}{{\n", ident);
        // Null array.
        switch (attribute.LengthSize)
        {
            case 1:
                WriteU8(builder, ident + Constants.DefaultIdent, 0, attribute);
                break;
            case 2:
                WriteU16(builder, ident + Constants.DefaultIdent, 0, attribute);
                break;
            case 4:
                WriteU32(builder, ident + Constants.DefaultIdent, 0, attribute);
                break;
            default:
                throw new NotImplementedException($"Length {attribute.LengthSize} not implemented");
        }
        builder.AppendFormat("{0}}} else {{\n", ident);
        // Array with items.
        switch (attribute.LengthSize)
        {
            case 1:
                WriteU8(builder, ident + Constants.DefaultIdent, $"Convert.ToByte({refName}.Length)", attribute);
                break;
            case 2:
                WriteU16(builder, ident + Constants.DefaultIdent, $"Convert.ToUInt16({refName}.Length)", attribute);
                break;
            case 4:
                WriteU32(builder, ident + Constants.DefaultIdent, $"Convert.ToUInt32({refName}.Length)", attribute);
                break;
            default:
                throw new NotImplementedException($"Length {attribute.LengthSize} not implemented");
        }
        builder.AppendFormat("{0}for (var i = 0; i < {1}.Length; i++)\n", ident + Constants.DefaultIdent, refName);
        builder.AppendFormat("{0}{{\n", ident + Constants.DefaultIdent);
        writeElement(ident + Constants.DefaultIdent + Constants.DefaultIdent, $"{refName}[i]");
        builder.AppendFormat("{0}}}\n", ident + Constants.DefaultIdent);
        builder.AppendFormat("{0}}}\n", ident);
    }
}