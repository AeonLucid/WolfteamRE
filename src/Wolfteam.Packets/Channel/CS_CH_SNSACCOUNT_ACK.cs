// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-24.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_CH_SNSACCOUNT_ACK)]
public partial class CS_CH_SNSACCOUNT_ACK : IWolfPacket
{
    /// <summary>
    ///     Set to 2.
    /// </summary>
    public byte Uk1 { get; set; }
    
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.ASCII)]
    public string? Uk2 { get; set; }
    
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.ASCII)]
    public string? Uk3 { get; set; }
}