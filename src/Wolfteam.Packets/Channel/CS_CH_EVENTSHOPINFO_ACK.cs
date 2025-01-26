// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-26.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_CH_EVENTSHOPINFO_ACK)]
public partial class CS_CH_EVENTSHOPINFO_ACK : IWolfPacket
{
    public byte Uk1_ArraySize { get; set; }
    
    public byte Uk2_ArraySize { get; set; }
    
    public byte Uk3 { get; set; }
}