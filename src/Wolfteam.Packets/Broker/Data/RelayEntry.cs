using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Broker.Data;

public partial class RelayEntry : IWolfPacket
{
    public byte Id { get; set; }
    public uint Address { get; set; }
    [WolfteamField(BigEndian = true)]
    public ushort Port { get; set; }
    public byte Padding { get; set; }
}