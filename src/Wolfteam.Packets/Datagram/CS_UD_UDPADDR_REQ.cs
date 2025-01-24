// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-24.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Datagram;

public partial class CS_UD_UDPADDR_REQ : IWolfPacket
{
    public ushort Uk1 { get; set; }
    public uint Uk2 { get; set; }
    public uint IpAddress { get; set; }
    
    [WolfteamField(BigEndian = true)]
    public ushort Port { get; set; }
}