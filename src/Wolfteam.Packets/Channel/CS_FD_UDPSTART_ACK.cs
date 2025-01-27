using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_FD_UDPSTART_ACK)]
public partial class CS_FD_UDPSTART_ACK : IWolfPacket
{
    public byte Uk1 { get; set; }
}