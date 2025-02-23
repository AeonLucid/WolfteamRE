﻿// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-27.

using System.Text;
using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Generator.CodeGen;

internal interface ICodeGen
{
    void WriteU8(StringBuilder builder, string ident, object? value, WolfteamFieldAttribute attribute);
    void WriteU16(StringBuilder builder, string ident, object? value, WolfteamFieldAttribute attribute);
    void WriteU32(StringBuilder builder, string ident, object? value, WolfteamFieldAttribute attribute);
    void WriteString(StringBuilder builder, string ident, string? refName, WolfteamFieldAttribute attribute);
    void WriteObject(StringBuilder builder, string ident, string? refName, string typeName, WolfteamFieldAttribute attribute);
    void WriteObjectArray(StringBuilder builder, string ident, string? refName, string typeName, WolfteamFieldAttribute attribute, ArrayElementDelegate writeElement);
}