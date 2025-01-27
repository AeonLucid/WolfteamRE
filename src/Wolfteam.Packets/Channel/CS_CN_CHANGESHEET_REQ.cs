// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-27.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_CN_CHANGESHEET_REQ)]
public partial class CS_CN_CHANGESHEET_REQ : IWolfPacket
{
    public byte Uk1 { get; set; }
}