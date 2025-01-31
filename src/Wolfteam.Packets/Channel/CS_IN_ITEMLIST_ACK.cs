// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-26.

using Wolfteam.Packets.Attributes;
using Wolfteam.Packets.Channel.Data;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_IN_ITEMLIST_ACK)]
public partial class CS_IN_ITEMLIST_ACK : IWolfPacket
{
    [WolfteamField(LengthSize = 1)]
    public ItemListUk1[]? Uk1 { get; set; }
    
    public byte Uk2 { get; set; }
}