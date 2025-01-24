﻿namespace Wolfteam.Packets;

public enum PacketId : ushort
{
    CS_BR_WORLDLIST_REQ = 0x1100,
    CS_BR_WORLDLIST_ACK = 0x1101,
    CS_BR_CHAINLIST_REQ = 0x1102,
    CS_BR_CHAINLIST_ACK = 0x1103,
    CS_BR_RELAYLIST_REQ = 0x1104,
    CS_BR_RELAYLIST_ACK = 0x1105,
    CS_BR_WORLDINFO_REQ = 0x1106,
    CS_BR_WORLDINFO_ACK = 0x1107,
    
    CS_CK_ALIVE_REQ = 0x1202,
    
    CS_CH_DISCONNECT_REQ = 0x1312,
}