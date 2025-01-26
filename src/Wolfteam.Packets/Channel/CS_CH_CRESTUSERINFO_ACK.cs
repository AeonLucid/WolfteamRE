// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-26.

using Wolfteam.Packets.Attributes;
using Wolfteam.Packets.Channel.Data;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_CH_CRESTUSERINFO_ACK)]
public partial class CS_CH_CRESTUSERINFO_ACK : IWolfPacket
{
    public ushort Unused { get; set; }
    
    [WolfteamField(LengthSize = 1)]
    public RestUserInfoUk1[]? Uk1 { get; set; }
    
    [WolfteamField(LengthSize = 2)]
    public RestUserInfoUk2[]? Uk2 { get; set; }
    
    public uint Uk3 { get; set; }
}