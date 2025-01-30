// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-30.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets;

[WolfteamPacket(PacketId.CS_FD_DETECTHACKTOOL_REQ)]
public partial class CS_FD_DETECTHACKTOOL_REQ : IWolfPacket
{
    public byte Uk1 { get; set; }
    
    public uint Uk2 { get; set; }
    
    public byte Uk3 { get; set; }
    
    public byte Uk4 { get; set; }
}