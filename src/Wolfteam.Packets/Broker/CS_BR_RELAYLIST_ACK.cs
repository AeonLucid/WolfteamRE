// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-22.

using Wolfteam.Packets.Attributes;
using Wolfteam.Packets.Broker.Data;

namespace Wolfteam.Packets.Broker;

[WolfteamPacket(PacketId.CS_BR_RELAYLIST_ACK)]
public partial class CS_BR_RELAYLIST_ACK : IWolfPacket
{
    public byte Padding { get; set; }
    
    [WolfteamField(LengthSize = 4)]
    public RelayEntry[]? Relays { get; set; }
}