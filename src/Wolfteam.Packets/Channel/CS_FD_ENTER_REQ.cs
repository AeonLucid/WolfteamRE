// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-27.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_FD_ENTER_REQ)]
public partial class CS_FD_ENTER_REQ : IWolfPacket
{
    public ushort FieldId { get; set; }
    
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.Unicode)]
    public string? Password { get; set; }
}