using Wolfteam.Packets.Attributes;
using Wolfteam.Packets.Broker.Data;

namespace Wolfteam.Packets.Broker;

[WolfteamPacket(PacketId.CS_BR_WORLDLIST_ACK)]
public partial class CS_BR_WORLDLIST_ACK : IWolfPacket
{
    [WolfteamField(LengthSize = 1)]
    public WorldListEntry[]? Entries { get; set; }
}