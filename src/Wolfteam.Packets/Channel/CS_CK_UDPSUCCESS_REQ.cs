// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-24.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_CK_UDPSUCCESS_REQ)]
public partial class CS_CK_UDPSUCCESS_REQ : IWolfPacket
{
    /// <summary>
    ///     Hardcoded to 0x00
    /// </summary>
    public byte Padding { get; set; }
}