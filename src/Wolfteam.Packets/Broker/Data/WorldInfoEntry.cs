using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Broker.Data;

public partial class WorldInfoEntry : IWolfPacket
{
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.Unicode)]
    public string? Name { get; set; }
    
    public ushort Kills { get; set; }
    
    public ushort Deaths { get; set; }
    
    public byte GP { get; set; }
    
    public byte Gold { get; set; }
}