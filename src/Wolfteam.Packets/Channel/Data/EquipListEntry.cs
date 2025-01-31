// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-31.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel.Data;

public partial class EquipListEntry : IWolfPacket
{
    public byte Uk1 { get; set; }
    
    [WolfteamField(LengthSize = 1)]
    public byte[]? Uk2 { get; set; }
    
    // Starting below, shared method with CS_CH_ITEMCHECK_ACK, CS_GI_PERMANENT_ITEM_CHANGE_TICKET_USE_ACK.
    
    public uint Uk3 { get; set; }
    
    public byte Uk4 { get; set; }
    
    public uint Uk5 { get; set; }
    
    public byte Uk6 { get; set; }
    
    public uint Uk7 { get; set; }
    
    public byte Uk8 { get; set; }
    
    public uint Uk9 { get; set; }
    
    public uint Uk10 { get; set; }
    
    /// <summary>
    ///     Only read when <see cref="Uk1"/> is 0.
    /// </summary>
    [WolfteamField(FixedSize = 15)]
    public uint[]? Uk11 { get; set; }
    
    public uint Uk12 { get; set; }
    
    /// <summary>
    ///     Only read when <see cref="Uk1"/> is 0.
    /// </summary>
    public byte Uk13 { get; set; }
}