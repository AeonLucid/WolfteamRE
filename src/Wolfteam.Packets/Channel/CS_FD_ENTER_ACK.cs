﻿// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-29.

using Wolfteam.Packets.Attributes;
using Wolfteam.Packets.Channel.Data;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_FD_ENTER_ACK)]
public partial class CS_FD_ENTER_ACK : IWolfPacket
{
    public uint Unused1 { get; set; }
    
    public ushort Unused2 { get; set; }
    
    public byte Slot { get; set; }
    
    public uint Unused3 { get; set; }
    
    public byte Team { get; set; }
    
    public byte Position { get; set; }
    
    public ushort ConnectionId { get; set; }
    
    public byte Uk6 { get; set; }
    
    public byte Uk7 { get; set; }
    
    public byte Class { get; set; }
    
    public uint Uk9 { get; set; }
    
    public uint Uk10 { get; set; }

    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.ASCII)]
    public string? Uk11 { get; set; }
    
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.Unicode)]
    public string? NickName { get; set; }
    
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.Unicode)]
    public string? Pride { get; set; }

    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.ASCII)]
    public string? Uk14 { get; set; }
    
    public ushort Uk15 { get; set; }
    
    /// <summary>
    ///     Limited to >= 4, investigate later.
    ///     Hardcode to 0 for now.
    /// </summary>
    [WolfteamField(LengthSize = 1, MaxSize = 4)]
    public FieldCharEntry4_Uk16[]? Uk16 { get; set; }
    
    [WolfteamField(LengthSize = 1)]
    public ushort[]? Uk17 { get; set; }
    
    [WolfteamField(LengthSize = 1)]
    public ushort[]? Uk18 { get; set; }
    
    public ushort Uk19 { get; set; }
    
    public byte Uk20 { get; set; }
    
    public byte Uk21 { get; set; }
    
    public byte Uk22 { get; set; }
    
    public uint RemoteIp { get; set; }
    
    [WolfteamField(BigEndian = true)]
    public ushort RemotePort { get; set; }
    
    public uint LocalIp { get; set; }
    
    [WolfteamField(BigEndian = true)]
    public ushort LocalPort { get; set; }
    
    [WolfteamField(LengthSize = 1)]
    public FieldCharEntry4_Sub[]? Uk27 { get; set; }
}