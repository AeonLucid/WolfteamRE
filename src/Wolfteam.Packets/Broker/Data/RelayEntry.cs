namespace Wolfteam.Packets.Broker.Data;

public partial class RelayEntry : IWolfPacket
{
    public byte Id { get; set; }
    public uint Address { get; set; }
    public ushort Port { get; set; }
    public byte Padding { get; set; }
}