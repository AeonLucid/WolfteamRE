// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-22.

using Wolfteam.Server.Buffers;

namespace Wolfteam.Server.Packets.Broker;

public class CS_BR_WORLDLIST_ACK : IWolfPacket
{
    private const int length = 14;
    
    public int CalculateSize()
    {
        return sizeof(byte) + (length * (sizeof(byte) + 
                                         sizeof(uint) + 
                                         sizeof(ushort) * 5));
    }

    public void Serialize(ref SpanWriter writer)
    {
        writer.WriteU8(length); // Length

        for (var i = 0; i < length; i++)
        {
            // DWORD  ipAddress
            writer.WriteU32(0x0100007F);
            // WORD   port
            writer.WriteU16(40850, bigEndian: true); // Availability?
            
            // BYTE   wlist_c
            writer.WriteU8(3);  // Must be 3??
            // WORD   wlist_d
            writer.WriteU16(ushort.MaxValue);
            // WORD   wlist_e
            writer.WriteU16(ushort.MaxValue);
            
            // WORD   wlist_f
            writer.WriteU16(5);  // Player count?
            // WORD   wlist_g
            writer.WriteU16(10); // Player count?
        }
    }

    public bool Deserialize(ref SpanReader reader)
    {
        throw new NotImplementedException();
    }
}