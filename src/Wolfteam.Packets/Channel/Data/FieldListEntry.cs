// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-27.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel.Data;

public partial class FieldListEntry : IWolfPacket
{
    public ushort FieldId { get; set; }
    
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.Unicode)]
    public string? Title { get; set; }
    
    public byte IsProtected { get; set; }
    
    public byte IsPlaying { get; set; }
    
    public byte Map { get; set; }
    
    public uint Mode { get; set; }
    
    public byte Uk7 { get; set; }
    
    public ushort Time { get; set; }
    
    public ushort Mission { get; set; }
    
    public byte Uk10 { get; set; }
    
    public byte Uk11 { get; set; }
    
    public ushort Uk12 { get; set; }
    
    public byte PlayerMax { get; set; }
    
    public byte PlayerCount { get; set; }
    
    public byte Uk15 { get; set; }
    
    public byte Uk16 { get; set; }
    
    public byte IsPowerRoom { get; set; }
    
    public byte ClassMax { get; set; }
    
    public byte ClassMin { get; set; }
}