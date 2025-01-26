// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-26.

namespace Wolfteam.Packets.Channel.Data;

public partial class RestUserInfoUk2 : IWolfPacket
{
    /// <summary>
    ///     Value of the last entry is used into another method.
    /// </summary>
    public ushort Uk1 { get; set; }
    
    public ushort Uk2 { get; set; }
    
    /// <summary>
    ///     Stops serializing array when this is not 0.
    /// </summary>
    public byte Uk3 { get; set; }
}