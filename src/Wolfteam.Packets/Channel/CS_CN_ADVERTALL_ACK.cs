using Wolfteam.Packets.Attributes;
using Wolfteam.Packets.Channel.Data;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_CN_ADVERTALL_ACK)]
public partial class CS_CN_ADVERTALL_ACK : IWolfPacket
{
    public ushort Uk1 { get; set; }
    
    [WolfteamField(LengthSize = 1)]
    public AdvertAllUk2[]? Uk2 { get; set; }
    
    public ushort Uk3 { get; set; }
    
    [WolfteamField(LengthSize = 1)]
    public AdvertAllUk4[]? Uk4 { get; set; }
    
    public ushort Uk5 { get; set; }
    
    [WolfteamField(LengthSize = 1)]
    public AdvertAllUk6[]? Uk6 { get; set; }
    
    public ushort Uk7 { get; set; }
    
    [WolfteamField(LengthSize = 1)]
    public AdvertAllUk8[]? Uk8 { get; set; }
}