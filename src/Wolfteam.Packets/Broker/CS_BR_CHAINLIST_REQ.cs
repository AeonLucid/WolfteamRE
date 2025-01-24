// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-22.

using System.Text;
using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Broker;

[WolfteamPacket(PacketId.CS_BR_CHAINLIST_REQ)]
public partial class CS_BR_CHAINLIST_REQ : IWolfPacket
{
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.ASCII)]
    public string? Username { get; set; }
    
    // public int Unknown1 { get; set; }
}