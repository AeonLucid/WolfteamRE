// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-26.

using Wolfteam.Packets.Attributes;
using Wolfteam.Packets.Channel.Data;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_IN_EQUIPLIST_ACK)]
public partial class CS_IN_EQUIPLIST_ACK : IWolfPacket
{
    [WolfteamField(LengthSize = 1, MaxSize = 4)]
    public EquipListEntry[]? Uk1 { get; set; }
}