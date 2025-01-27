using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel.Data;

public partial class AdvertAllUk6 : IWolfPacket
{
    public uint Uk1 { get; set; }
    
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.Unicode)]
    public string? Uk2 { get; set; }
    
    public byte Uk3 { get; set; }
    
    public ushort Uk4 { get; set; }
}