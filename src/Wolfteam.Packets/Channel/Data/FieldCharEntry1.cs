// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-27.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel.Data;

public partial class FieldCharEntry1 : IWolfPacket
{
    public byte Uk1 { get; set; }
    
    public uint Unused { get; set; }
    
    public byte Uk2 { get; set; }
    
    public byte Uk3 { get; set; }
    
    // Inside method
    
    public byte Uk4 { get; set; }
    
    public byte Uk5 { get; set; }
    
    public byte Unused2 { get; set; }
    
    public uint Uk6 { get; set; }
    
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.Unicode)]
    public string? Uk7 { get; set; }
    
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.Unicode)]
    public string? Uk8 { get; set; }
    
    public byte Uk9 { get; set; }
    
    public uint Uk10 { get; set; }
}