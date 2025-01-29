// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-29.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_FD_CHANGETEAM_REQ)]
public partial class CS_FD_CHANGETEAM_REQ : IWolfPacket
{
    public byte Uk1 { get; set; }
}