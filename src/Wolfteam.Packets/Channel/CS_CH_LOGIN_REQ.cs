// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-24.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_CH_LOGIN_REQ)]
public partial class CS_CH_LOGIN_REQ : IWolfPacket
{
    public ushort ClientVersion { get; set; }
    
    /// <summary>
    ///     MD5 hash of files.
    /// </summary>
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.ASCII)]
    public string? FilesHash { get; set; }
    
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.ASCII)]
    public string? Username { get; set; }
    
    [WolfteamField(LengthSize = 2, Encoding = FieldEncoding.ASCII)]
    public string? PasswordHash { get; set; }
    
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.ASCII)]
    public string? HardwareHash { get; set; }
    
    public uint Padding { get; set; }
    
    [WolfteamField(Length = 32, Encoding = FieldEncoding.ASCII)]
    public string? TmpDllHashes { get; set; }
}