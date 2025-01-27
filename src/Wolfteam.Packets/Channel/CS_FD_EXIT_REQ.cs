// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-27.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_FD_EXIT_REQ)]
public partial class CS_FD_EXIT_REQ : IWolfPacket
{
    public ushort Uk1 { get; set; }
    
    public byte Uk2 { get; set; }
    
    public ushort Uk3 { get; set; }
}