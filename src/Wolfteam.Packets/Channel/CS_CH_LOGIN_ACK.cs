using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_CH_LOGIN_ACK)]
public partial class CS_CH_LOGIN_ACK : IWolfPacket
{
    public ushort Uk1 { get; set; }
    public uint Uk2 { get; set; }
    public uint Uk3 { get; set; }
    public uint Uk4 { get; set; }
    public ushort Uk5 { get; set; }
    public uint Uk6 { get; set; }
    /// <summary>
    ///     Boolean
    /// </summary>
    public byte Uk7 { get; set; }
    public byte Uk8 { get; set; }
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.ASCII)]
    public string? Uk9 { get; set; }
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.ASCII)]
    public string? Uk10 { get; set; }
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.ASCII)]
    public string? Uk11 { get; set; }
    /// <summary>
    ///     Boolean
    /// </summary>
    public byte Uk12 { get; set; }
    public byte Uk13 { get; set; }
    public byte Uk14 { get; set; }
    public byte Uk15 { get; set; }
}