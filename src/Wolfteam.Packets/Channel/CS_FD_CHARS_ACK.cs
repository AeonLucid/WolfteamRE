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
    public string? Title { get; set; }
    
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.Unicode)]
    public string? Password { get; set; }
    
    public byte MapId { get; set; }
    
    /// <summary>
    ///     When `Value & 0x100200 == 0x100200` it serializes different char entrys.
    /// </summary>
    public uint Mode { get; set; }
    
    public byte Wins { get; set; }
    
    public byte UnusedArray { get; set; }
    
    public ushort Time { get; set; }
    
    public ushort Mission { get; set; }
    
    public byte FlagTresspass { get; set; }
    
    public byte FlagHero { get; set; }
    
    public ushort FlagHeavy { get; set; }
    
    public byte PlayerLimit { get; set; }
    
    public byte Uk16 { get; set; }
    
    public byte Uk17 { get; set; }
    
    public byte Uk18 { get; set; }
    
    public byte IsPowerRoom { get; set; }
    
    [WolfteamField(LengthSize = 1)]
    public FieldCharEntry4[]? Chars { get; set; }
    
    public byte ClassMax { get; set; }
    
    public byte ClassMin { get; set; }
}