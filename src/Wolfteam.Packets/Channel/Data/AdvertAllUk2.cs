using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel.Data;

public partial class AdvertAllUk2 : IWolfPacket
{
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.Unicode)]
    public string? Uk1 { get; set; }
}