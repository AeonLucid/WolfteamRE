// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-30.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_FD_CHANGESLOTSTATUS_ACK)]
public partial class CS_FD_CHANGESLOTSTATUS_ACK : IWolfPacket
{
    public byte Slot { get; set; }
    
    public byte Status { get; set; }
}