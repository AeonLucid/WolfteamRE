// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-30.

using Wolfteam.Packets.Attributes;
using Wolfteam.Packets.Channel.Data;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_FD_STARTROUND_ACK)]
public partial class CS_FD_STARTROUND_ACK : IWolfPacket
{
    public byte Uk1 { get; set; }
    
    public ushort Uk2 { get; set; }
    
    public byte Uk3 { get; set; }
    
    public byte Uk4 { get; set; }
    
    [WolfteamField(LengthSize = 1)]
    public StartRoundUk5[]? Uk5 { get; set; }
}