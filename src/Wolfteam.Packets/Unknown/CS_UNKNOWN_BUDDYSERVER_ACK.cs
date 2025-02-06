// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-02-05.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Unknown;

[WolfteamPacket(PacketId.CS_UNKNOWN_BUDDYSERVER_ACK)]
public partial class CS_UNKNOWN_BUDDYSERVER_ACK : IWolfPacket
{
    public uint IpAddress { get; set; }
    
    [WolfteamField(BigEndian = true)]
    public ushort Port { get; set; }
}