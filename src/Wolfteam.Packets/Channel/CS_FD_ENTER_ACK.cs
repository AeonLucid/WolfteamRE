// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-29.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_FD_ENTER_ACK)]
public partial class CS_FD_ENTER_ACK : IWolfPacket
{
    public uint Unused1 { get; set; }
    
    public ushort Unused2 { get; set; }
    
    public byte Uk1 { get; set; }
    
    public uint Uk2 { get; set; }
    
    public byte Uk3 { get; set; }
    
    public byte Uk4 { get; set; }
    
    public ushort Uk5 { get; set; }
    
    public byte Uk6 { get; set; }
    
    public byte Uk7 { get; set; }
    
    public byte Uk8 { get; set; }
    
    public uint Uk9 { get; set; }
    
    public uint Uk10 { get; set; }

    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.ASCII)]
    public string? Uk11 { get; set; }
    
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.Unicode)]
    public string? Uk12 { get; set; }
    
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.Unicode)]
    public string? Uk13 { get; set; }

    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.ASCII)]
    public string? Uk14 { get; set; }
    
    public ushort Uk15 { get; set; }
    
    /// <summary>
    ///     Limited to >= 4, investigate later.
    ///     Hardcode to 0 for now.
    /// </summary>
    public byte Uk16 { get; set; }
    
    [WolfteamField(LengthSize = 1)]
    public ushort[]? Uk17 { get; set; }
    
    [WolfteamField(LengthSize = 1)]
    public ushort[]? Uk18 { get; set; }
    
    public ushort Uk19 { get; set; }
    
    public byte Uk20 { get; set; }
    
    public byte Uk21 { get; set; }
    
    public byte Uk22 { get; set; }
    
    public uint Uk23 { get; set; }
    
    public ushort Uk24 { get; set; }
    
    public uint Uk25 { get; set; }
    
    public ushort Uk26 { get; set; }
    
    public byte Uk27_ArraySize { get; set; }
}