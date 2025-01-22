// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-22.

using Wolfteam.Server.Buffers;

namespace Wolfteam.Server.Packets.Broker;

public class CS_BR_WORLDINFO_REQ : IWolfPacket
{
    public ushort Unknown { get; set; }
    
    public int CalculateSize()
    {
        return 2;
    }

    public void Serialize(ref SpanWriter writer)
    {
        writer.WriteU16(Unknown);
    }

    public bool Deserialize(ref SpanReader reader)
    {
        if (!reader.TryReadU16(out var unknown))
        {
            return false;
        }
        
        Unknown = unknown;
        return true;
    }
}