// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-27.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_FD_CREATE_REQ)]
public partial class CS_FD_CREATE_REQ : IWolfPacket
{
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.Unicode)]
    public string? Title { get; set; }
    
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.Unicode)]
    public string? Password { get; set; }
    
    public byte MapId { get; set; }
    
    public uint Mode { get; set; }
    
    public byte Uk4 { get; set; }
    
    public byte Uk5 { get; set; }
    
    public ushort Time { get; set; }
    
    public ushort Mission { get; set; }
    
    public byte FlagTresspass { get; set; }
    
    public byte FlagHero { get; set; }
    
    public ushort FlagHeavy { get; set; }
    
    public byte PlayerLimit { get; set; }
    
    public byte Uk12 { get; set; }
    
    public byte Uk13 { get; set; }
    
    public ushort Uk14 { get; set; }
    
    public ushort Uk15 { get; set; }
    
    public byte Uk16 { get; set; }
    
    public byte Uk17 { get; set; }
    
    public byte ClassMax { get; set; }
    
    public byte ClassMin { get; set; }
    
    public ushort Uk20 { get; set; }
}