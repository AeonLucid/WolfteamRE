// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-26.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_IN_PACKAGE_ITEM_LIST_ACK)]
public partial class CS_IN_PACKAGE_ITEM_LIST_ACK : IWolfPacket
{
    [WolfteamField(LengthSize = 1)]
    public ushort[]? Uk1 { get; set; }
    
    /// <summary>
    ///     Stops serializing when value is 0.
    /// </summary>
    public byte Uk2_ArraySize { get; set; }
}