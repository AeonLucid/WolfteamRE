// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-26.

namespace Wolfteam.Packets.Channel.Data;

public partial class RestUserInfoUk1 : IWolfPacket
{
    public byte Uk1 { get; set; }
    
    public ushort Uk2 { get; set; }
}