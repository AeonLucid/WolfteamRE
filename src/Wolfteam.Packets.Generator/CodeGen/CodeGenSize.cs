// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-27.

using System;
using System.Text;
using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Generator.CodeGen;

internal class CodeGenSize : ICodeGen
{
    public void WriteU8(StringBuilder builder, string ident, object? value, WolfteamFieldAttribute attribute)
    {
        builder.AppendFormat("{0}size += {1};\n", ident, sizeof(byte));
    }

    public void WriteU16(StringBuilder builder, string ident, object? value, WolfteamFieldAttribute attribute)
    {
        builder.AppendFormat("{0}size += {1};\n", ident, sizeof(ushort));
    }

    public void WriteU32(StringBuilder builder, string ident, object? value, WolfteamFieldAttribute attribute)
    {
        builder.AppendFormat("{0}size += {1};\n", ident, sizeof(uint));
    }

    public void WriteString(StringBuilder builder, string ident, string? refName, WolfteamFieldAttribute attribute)
    {
        if (string.IsNullOrEmpty(refName))
        {
            throw new ArgumentNullException(nameof(refName));
        }

        if (attribute.FixedSize > 0)
        {
            builder.AppendFormat("{0}size += {1};\n", ident, attribute.FixedSize);
            return;
        }
        
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

    public void WriteObject(StringBuilder builder, string ident, string? refName, string typeName, WolfteamFieldAttribute attribute)
    {
        if (string.IsNullOrEmpty(refName))
        {
            throw new ArgumentNullException(nameof(refName));
        }
        
        builder.AppendFormat("{0}size += {1}.Size(version);\n", ident, refName);
    }

    public void WriteObjectArray(StringBuilder builder, string ident, string? refName, string typeName, WolfteamFieldAttribute attribute, ArrayElementDelegate writeElement)
    {
        if (string.IsNullOrEmpty(refName))
        {
            throw new ArgumentNullException(nameof(refName));
        }

        if (attribute.FixedSize > 0)
        {
            builder.AppendFormat("{0}for (var i = 0; i < {1}; i++)\n", ident, attribute.FixedSize);
            builder.AppendFormat("{0}{{\n", ident);
            // We set the ref to null because there is no array entry to reference.
            // Let WriteObject deal with it, should probably create an instance and get the size of that.
            writeElement(ident + Constants.DefaultIdent, string.Empty);
            builder.AppendFormat("{0}}}\n", ident);
            return;
        }
        
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