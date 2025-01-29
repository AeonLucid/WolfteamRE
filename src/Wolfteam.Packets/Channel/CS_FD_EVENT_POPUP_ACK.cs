﻿// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-29.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_FD_EVENT_POPUP_ACK)]
public partial class CS_FD_EVENT_POPUP_ACK : IWolfPacket
{
    public ushort Uk1 { get; set; }
}