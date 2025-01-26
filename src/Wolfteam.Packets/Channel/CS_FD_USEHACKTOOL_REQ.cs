// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-26.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_FD_USEHACKTOOL_REQ)]
public partial class CS_FD_USEHACKTOOL_REQ : IWolfPacket
{
    public uint Uk1 { get; set; }
    
    public uint Uk2 { get; set; }
    
    public ushort Uk3 { get; set; }
    
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.Unicode)]
    public string? Uk4 { get; set; }
}