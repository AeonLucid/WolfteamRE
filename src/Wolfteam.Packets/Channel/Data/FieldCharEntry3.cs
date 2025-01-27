// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-27.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel.Data;

public partial class FieldCharEntry3 : IWolfPacket
{
    public byte Uk1 { get; set; }
    
    public uint Unused { get; set; }
    
    public byte Uk2 { get; set; }
    
    public byte Uk3 { get; set; }
    
    // Inside method
    
    // TODO: Implement
}