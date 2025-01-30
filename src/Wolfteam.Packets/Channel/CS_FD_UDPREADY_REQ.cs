// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-30.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_FD_UDPREADY_REQ)]
public partial class CS_FD_UDPREADY_REQ : IWolfPacket
{
    public byte Ready { get; set; }
}