// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-26.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_IN_CHARITEMLIST_ACK)]
public partial class CS_IN_CHARITEMLIST_ACK : IWolfPacket
{
    /// <summary>
    ///     Some boolean
    /// </summary>
    public byte Uk1_Bool { get; set; }
    
    public uint Uk2_ArraySize { get; set; }
}