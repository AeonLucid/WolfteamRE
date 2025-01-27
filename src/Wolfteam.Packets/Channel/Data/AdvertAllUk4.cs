using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel.Data;

public partial class AdvertAllUk4 : IWolfPacket
{
    public byte Uk1 { get; set; }
    
    public byte Uk2 { get; set; }
    
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.Unicode)]
    public string? Uk3 { get; set; }
    
    [WolfteamField(LengthSize = 1, Encoding = FieldEncoding.Unicode)]
    public string? Uk4 { get; set; }
    
    public uint Uk5 { get; set; }
    
    public uint Uk6 { get; set; }
    
    public byte Uk7 { get; set; }
    
    public byte Uk8 { get; set; }
}