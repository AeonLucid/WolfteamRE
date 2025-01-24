using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Broker.Data;

public partial class WorldListEntry : IWolfPacket
{
    public uint IpAddress { get; set; }
    
    [WolfteamField(BigEndian = true)]
    public ushort Port { get; set; }
    
    public byte Uk1 { get; set; }
    
    public ushort Uk2 { get; set; }
    
    public ushort Uk3 { get; set; }
    
    public ushort PlayerCount1 { get; set; }
    
    public ushort PlayerCount2 { get; set; }
    
    [WolfteamField(Version = ClientVersion.Reboot, LengthSize = 1, Encoding = FieldEncoding.ASCII)]
    public string? ServerDns { get; set; }
}