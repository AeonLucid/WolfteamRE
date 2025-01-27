// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-27.

using Wolfteam.Packets.Attributes;
using Wolfteam.Packets.Channel.Data;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_FD_CHARS_ACK)]
public partial class CS_FD_CHARS_ACK : IWolfPacket
{
    public byte Uk1 { get; set; }
    
    public byte OwnerCharId { get; set; }
    
    public ushort FieldId { get; set; }
    
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.Unicode)]
    public string? Uk4 { get; set; }
    
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.Unicode)]
    public string? Uk5 { get; set; }
    
    public byte Uk6 { get; set; }
    
    /// <summary>
    ///     When `Value & 0x100200 == 0x100200` it serializes different char entrys.
    /// </summary>
    public uint Uk7 { get; set; }
    
    public byte Uk8 { get; set; }
    
    public byte UnusedArray { get; set; }
    
    public ushort Uk10 { get; set; }
    
    public ushort Uk11 { get; set; }
    
    public byte FlagTresspass { get; set; }
    
    public byte FlagHero { get; set; }
    
    public ushort FlagHeavy { get; set; }
    
    public byte PlayerLimit { get; set; }
    
    public byte Uk16 { get; set; }
    
    public byte Uk17 { get; set; }
    
    public byte Uk18 { get; set; }
    
    public byte IsPowerRoom { get; set; }
    
    [WolfteamField(LengthSize = 1)]
    public FieldCharEntry4[]? Uk20 { get; set; }
    
    public byte Uk21 { get; set; }
    
    public byte Uk22 { get; set; }
}