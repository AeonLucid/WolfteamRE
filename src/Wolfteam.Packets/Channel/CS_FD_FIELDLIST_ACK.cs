// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-27.

using Wolfteam.Packets.Attributes;
using Wolfteam.Packets.Channel.Data;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_FD_FIELDLIST_ACK)]
public partial class CS_FD_FIELDLIST_ACK : IWolfPacket
{
    public byte Uk1 { get; set; }
    
    public byte Uk2 { get; set; }
    
    [WolfteamField(LengthSize = 1)]
    public FieldListEntry[]? Uk3 { get; set; }
}