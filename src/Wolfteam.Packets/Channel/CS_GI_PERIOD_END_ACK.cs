// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-29.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_GI_PERIOD_END_ACK)]
public partial class CS_GI_PERIOD_END_ACK : IWolfPacket
{
    public byte Uk1_ArraySize { get; set; }
}