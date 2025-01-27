// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-27.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_FD_CREATE_ACK)]
public partial class CS_FD_CREATE_ACK : IWolfPacket
{
    public uint Unused { get; set; }
    
    public ushort Uk2 { get; set; }
    
    public byte Uk3 { get; set; }
    
    public byte Uk4 { get; set; }
}