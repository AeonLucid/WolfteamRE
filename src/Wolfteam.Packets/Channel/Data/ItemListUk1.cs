// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-31.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel.Data;

public partial class ItemListUk1 : IWolfPacket
{
    public uint UniqueId { get; set; }
    
    public ushort ItemId { get; set; }
    
    // Array of 4 shorts without a length.
    
    public ushort Uk3_1 { get; set; }
    
    public ushort Uk3_2 { get; set; }
    
    public ushort Uk3_3 { get; set; }
    
    public ushort Uk3_4 { get; set; }
    
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.ASCII)]
    public string? Expiry { get; set; }
    
    public ushort DaysLeft { get; set; }
    
    public byte Uk6 { get; set; }
    
    public byte Uk7 { get; set; }
}