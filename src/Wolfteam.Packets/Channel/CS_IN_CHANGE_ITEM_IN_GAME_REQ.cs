// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-30.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_IN_CHANGE_ITEM_IN_GAME_REQ)]
public partial class CS_IN_CHANGE_ITEM_IN_GAME_REQ : IWolfPacket
{
    public byte Uk1 { get; set; }
    
    public byte Uk2 { get; set; }
    
    public byte Uk3 { get; set; }
    
    public uint Uk4 { get; set; }
    
    public ushort Uk5 { get; set; }
    
    public byte Uk6 { get; set; }
}