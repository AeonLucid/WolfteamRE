using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel.Data;

public partial class AdvertAllUk8 : IWolfPacket
{
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.Unicode)]
    public string? Uk1 { get; set; }
    
    public ushort Uk2 { get; set; }
}