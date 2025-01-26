// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-26.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_CH_SELECTCHAR_ACK)]
public partial class CS_CH_SELECTCHAR_ACK : IWolfPacket
{
    public uint Uk1 { get; set; }
    
    /// <summary>
    ///     Checked if != 0xFFFF
    /// </summary>
    public ushort Uk2 { get; set; }
    
    public byte Uk3_ArraySize { get; set; }
}