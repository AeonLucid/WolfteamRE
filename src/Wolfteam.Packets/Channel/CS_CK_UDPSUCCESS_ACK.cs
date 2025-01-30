// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-24.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_CK_UDPSUCCESS_ACK)]
public partial class CS_CK_UDPSUCCESS_ACK : IWolfPacket
{
    public uint RemoteIp { get; set; }
    [WolfteamField(BigEndian = true)]
    public ushort RemotePort { get; set; }
    public uint LocalIp { get; set; }
    [WolfteamField(BigEndian = true)]
    public ushort LocalPort { get; set; }
}