// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-22.

using Wolfteam.Server.Buffers;

namespace Wolfteam.Server.Packets.Broker;

public class CS_BR_RELAYLIST_ACK : IWolfPacket
{
    private const int length = 14;
    
    public int CalculateSize()
    {
        return sizeof(byte) + sizeof(uint) + (length * (sizeof(byte) + 
                                         sizeof(uint) + 
                                         sizeof(ushort) +
                                         sizeof(byte)));
    }

    public void Serialize(ref SpanWriter writer)
    {
        // DWORD length
        writer.WriteU8(0); // Filler
        writer.WriteU32(length);

        for (byte i = 0; i < length; i++)
        {
            writer.WriteU8(i);
            writer.WriteU32(0x0100007F); // 127.0.0.1
            writer.WriteU16(40800);      // Port
            writer.WriteU8(0); // Filler
        }
    }

    public bool Deserialize(ref SpanReader reader)
    {
        throw new NotImplementedException();
    }
}