// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-27.

using System;
using System.Text;
using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Generator.CodeGen;

internal class CodeGenDeserialize : ICodeGen
{
    public const string IntValue = "uintValue";
    public const string ShortValue = "ushortValue";
    public const string ByteValue = "byteValue";

    public void WriteU8(StringBuilder builder, string ident, object value, WolfteamFieldAttribute attribute)
    {
        builder.AppendFormat("{0}if (!reader.TryReadU8(out {1})) {{\n", ident, ByteValue);
        builder.AppendFormat("{0}return false;\n", ident + Constants.DefaultIdent);
        builder.AppendFormat("{0}}}\n", ident);
            
        if (!ByteValue.Equals(value))
        {
            builder.AppendFormat("{0}{1} = {2};\n", ident, value, ByteValue);
        }
    }

    public void WriteU16(StringBuilder builder, string ident, object value, WolfteamFieldAttribute attribute)
    {
        builder.AppendFormat(attribute.BigEndian
                ? "{0}if (!reader.TryReadU16BE(out {1})) {{\n"
                : "{0}if (!reader.TryReadU16(out {1})) {{\n", ident, ShortValue);

        builder.AppendFormat("{0}return false;\n", ident + Constants.DefaultIdent);
        builder.AppendFormat("{0}}}\n", ident);
            
        if (!ShortValue.Equals(value))
        {
            builder.AppendFormat("{0}{1} = {2};\n", ident, value, ShortValue);
        }
    }

    public void WriteU32(StringBuilder builder, string ident, object value, WolfteamFieldAttribute attribute)
    {
        builder.AppendFormat("{0}if (!reader.TryReadU32(out {1})) {{\n", ident, IntValue);
        builder.AppendFormat("{0}return false;\n", ident + Constants.DefaultIdent);
        builder.AppendFormat("{0}}}\n", ident);
            
        if (!IntValue.Equals(value))
        {
            builder.AppendFormat("{0}{1} = {2};\n", ident, value, IntValue);
        }
    }

    public void WriteString(StringBuilder builder, string ident, string refName, WolfteamFieldAttribute attribute)
    {
        // Read length.
        string lengthRef;
        
        if (attribute.Length != 0)
        {
            lengthRef = attribute.Length.ToString();
        }
        else
        {
            switch (attribute.LengthSize)
            {
                case 1:
                    lengthRef = ByteValue;
                    WriteU8(builder, ident, ByteValue, attribute);
                    break;
                case 2:
                    lengthRef = ShortValue;
                    WriteU16(builder, ident, ShortValue, attribute);
                    break;
                case 4:
                    lengthRef = IntValue;
                    WriteU32(builder, ident, IntValue, attribute);
                    break;
                default:
                    throw new NotImplementedException($"Length {attribute.LengthSize} not implemented");
            }
        }
            
        // Read string.
        var outRef = $"out{refName}";
            
        builder.AppendFormat("{0}if (!reader.TryReadString(Encoding.{1}, {2}, out var {3}))\n", ident, attribute.Encoding, lengthRef, outRef);
        builder.AppendFormat("{0}{{\n", ident);
        builder.AppendFormat("{0}return false;\n", ident + Constants.DefaultIdent);
        builder.AppendFormat("{0}}}\n", ident);
        builder.AppendFormat("{0}{1} = {2};\n", ident, refName, outRef);
    }

    public void WriteObject(StringBuilder builder, string ident, string refName, string typeName, WolfteamFieldAttribute attribute)
    {
        builder.AppendFormat("{0}{1} = new {2}();\n", ident, refName, typeName);
        builder.AppendFormat("{0}{1}.Deserialize(version, ref reader);\n", ident, refName);
    }

    public void WriteObjectArray(StringBuilder builder, string ident, string refName, string typeName, WolfteamFieldAttribute attribute, ArrayElementDelegate writeElement)
    {
        // Read length.
        string lengthRef;
            
        switch (attribute.LengthSize)
        {
            case 1:
                lengthRef = ByteValue;
                WriteU8(builder, ident, ByteValue, attribute);
                break;
            case 2:
                lengthRef = ShortValue;
                WriteU16(builder, ident, ShortValue, attribute);
                break;
            case 4:
                lengthRef = IntValue;
                WriteU32(builder, ident, IntValue, attribute);
                break;
            default:
                throw new NotImplementedException($"Length {attribute.LengthSize} not implemented");
        }
            
        builder.AppendFormat("{0}{1} = new {2}[{3}];\n", ident, refName, typeName, lengthRef);
        builder.AppendFormat("{0}for (var i = 0; i < {1}.Length; i++)\n", ident, refName);
        builder.AppendFormat("{0}{{\n", ident);
        writeElement(ident + Constants.DefaultIdent, $"{refName}[i]");
        builder.AppendFormat("{0}}}\n", ident);
    }
}