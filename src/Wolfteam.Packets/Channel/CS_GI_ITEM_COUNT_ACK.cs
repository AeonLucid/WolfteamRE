﻿// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-29.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_GI_ITEM_COUNT_ACK)]
public partial class CS_GI_ITEM_COUNT_ACK : IWolfPacket
{
    public ushort Uk1 { get; set; }
    
    public ushort Uk2 { get; set; }
}