using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel.Data;

public partial class UserInfoUk42 : IWolfPacket
{
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.ASCII)]
    public string? Uk1 { get; set; }
}