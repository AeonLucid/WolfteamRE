// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-22.

using Wolfteam.Packets.Attributes;
using Wolfteam.Packets.Broker.Data;

namespace Wolfteam.Packets.Broker;

[WolfteamPacket(PacketId.CS_BR_WORLDINFO_ACK)]
public partial class CS_BR_WORLDINFO_ACK : IWolfPacket
{
    [WolfteamField(LengthSize = 1)]
    public WorldInfoEntry[]? WorldInfoEntries { get; set; }
}