// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-22.

using System.Text;
using Wolfteam.Server.Buffers;

namespace Wolfteam.Server.Packets.Broker;

public class CS_BR_WORLDINFO_ACK : IWolfPacket
{
    private const int length = 14;
    
    public int CalculateSize()
    {
        return sizeof(byte) + 
               (length * (sizeof(byte) + 
                          8 +
                          sizeof(ushort) + 
                          sizeof(ushort) + 
                          sizeof(byte) + 
                          sizeof(byte)));
    }

    public void Serialize(ref SpanWriter writer)
    {
        writer.WriteU8(length); // array length

        for (var i = 0; i < length; i++)
        {
            writer.WriteString(Encoding.Unicode, "Test");
            writer.WriteU16(100); // Kills (divided by 100)
            writer.WriteU16(100); // Death (divided by 100)
            writer.WriteU8(6);  // GP %
            writer.WriteU8(10); // Gold %
        }
    }

    public bool Deserialize(ref SpanReader reader)
    {
        throw new NotImplementedException();
    }
}