using Wolfteam.Packets.Attributes;
using Wolfteam.Packets.Broker.Data;
using UserInfoUk42 = Wolfteam.Packets.Channel.Data.UserInfoUk42;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_CH_USERINFO_ACK)]
public partial class CS_CH_USERINFO_ACK : IWolfPacket
{
    /// <summary>
    ///     TODO: Max length 64, otherwise causes buffer overflow.
    ///     TODO: Check if Unicode.
    /// </summary>
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.Unicode)]
    public string? NickName { get; set; }
    
    public byte Uk2 { get; set; }
    
    public byte Uk3 { get; set; }
    
    public uint Uk4 { get; set; }
    
    public uint CurrencyGold { get; set; }
    
    public uint CurrencyWC { get; set; }
    
    public uint CurrencyCash { get; set; }
    
    public uint Uk8 { get; set; }
    
    public uint Uk9 { get; set; }
    
    public uint Uk10 { get; set; }
    
    public byte Uk11 { get; set; }
    
    public byte Uk12 { get; set; }
    
    public uint Ranking { get; set; }
    
    public uint Uk14 { get; set; }
    
    public uint Uk15 { get; set; }
    
    public uint Uk16 { get; set; }
    
    public uint Uk17 { get; set; }
    
    public uint Uk18 { get; set; }
    
    public uint Kill { get; set; }
    
    public uint Death { get; set; }
    
    public uint Uk21 { get; set; }
    
    public uint Uk22 { get; set; }
    
    public uint Uk23 { get; set; }
    
    public uint Uk24 { get; set; }
    
    public byte Uk25 { get; set; }
    
    public uint Uk26 { get; set; }
    
    /// <summary>
    ///     TODO: Max length ??
    ///     TODO: Check if Unicode.
    /// </summary>
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.Unicode)]
    public string? Uk27 { get; set; }
    
    /// <summary>
    ///     TODO: Packet stops here if <see cref="Uk28"/> is 0.
    /// </summary>
    public uint Uk28 { get; set; }
    
    /// <summary>
    ///     TODO: Max length ??
    ///     TODO: Check if Unicode.
    /// </summary>
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.Unicode)]
    public string? PrideName { get; set; }
    
    /// <summary>
    ///     TODO: Max length ??
    ///     TODO: Check if Unicode.
    /// </summary>
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.Unicode)]
    public string? Uk30 { get; set; }
    
    public uint Uk31 { get; set; }
    
    public byte Uk32 { get; set; }
    
    /// <summary>
    ///     TODO: Max length ??
    ///     Not unicode probably
    ///     TODO: Is compared against an hash, don't know what yet.
    /// </summary>
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.ASCII)]
    public string? Uk33 { get; set; }
    
    public ushort Uk34 { get; set; }
    
    /// <summary>
    ///     TODO: Max length ??
    ///     TODO: Check if Unicode.
    /// </summary>
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.Unicode)]
    public string? Uk35 { get; set; }
    
    /// <summary>
    ///     TODO: Max length ??
    ///     TODO: Check if Unicode.
    /// </summary>
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.Unicode)]
    public string? Uk36 { get; set; }
    
    /// <summary>
    ///     Serialize next field if <see cref="Uk37"/> is >= 10.
    /// </summary>
    public byte Uk37 { get; set; }
    
    /// <summary>
    ///     TODO: Max length ??
    ///     Not unicode probably
    /// </summary>
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.ASCII)]
    public string? Uk38 { get; set; }
    
    public byte Uk39 { get; set; }
    
    public byte Uk40 { get; set; }
    
    /// <summary>
    ///     TODO: Max length ??
    ///     Not unicode probably
    /// </summary>
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.ASCII)]
    public string? Uk41 { get; set; }
    
    [WolfteamField(LengthSize = 1)]
    public UserInfoUk42[]? Uk42 { get; set; }

    [WolfteamField(LengthSize = 1)]
    public ushort[]? Uk43 { get; set; }
}