// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-27.

namespace Wolfteam.Packets.Channel.Data;

public partial class FieldCharEntry2 : IWolfPacket
{
    public byte Uk1 { get; set; }
    
    public uint Unused { get; set; }
    
    public byte Uk2 { get; set; }
    
    public byte Uk3 { get; set; }
    
    // Inside method
    
    public byte Uk4 { get; set; }
    
    public byte Uk5 { get; set; }
    
    public byte Uk6 { get; set; }
    
    public byte Uk7 { get; set; }
    
    public uint Unused1 { get; set; }
    
    public uint Unused2 { get; set; }
    
    public uint Unused3 { get; set; }
    
    public byte Uk8 { get; set; }
    
    public byte Uk9 { get; set; }
}