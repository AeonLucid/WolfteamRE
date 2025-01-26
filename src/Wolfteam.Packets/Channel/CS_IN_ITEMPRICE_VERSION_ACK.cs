// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-26.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_IN_ITEMPRICE_VERSION_ACK)]
public partial class CS_IN_ITEMPRICE_VERSION_ACK : IWolfPacket
{
    public uint Uk1 { get; set; }
    
    public uint Uk2 { get; set; }
}