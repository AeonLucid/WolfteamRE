// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-22.

using Wolfteam.Server.Buffers;

namespace Wolfteam.Server.Packets.Broker;

public class CS_BR_CHAINLIST_ACK : IWolfPacket
{
    public byte[]? Chainlist { get; set; }
    
    public int CalculateSize()
    {
        if (Chainlist == null)
        {
            throw new InvalidOperationException($"{nameof(Chainlist)} is null.");
        }
        
        return 1 + Chainlist.Length;
    }

    public void Serialize(ref SpanWriter writer)
    {
        if (Chainlist == null)
        {
            throw new InvalidOperationException($"{nameof(Chainlist)} is null.");
        }
        
        writer.WriteU8((byte)Chainlist.Length);

        foreach (var grade in Chainlist)
        {
            writer.WriteU8(grade);
        }
    }

    public bool Deserialize(ref SpanReader reader)
    {
        throw new NotImplementedException();
    }
}