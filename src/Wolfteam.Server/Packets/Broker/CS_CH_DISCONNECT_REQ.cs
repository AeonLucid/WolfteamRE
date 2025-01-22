// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-22.

using Wolfteam.Server.Buffers;

namespace Wolfteam.Server.Packets.Broker;

public class CS_CH_DISCONNECT_REQ : IWolfPacket
{
    public byte[]? Unknown { get; set; }
    
    public int CalculateSize()
    {
        throw new NotImplementedException();
    }

    public void Serialize(SpanWriter writer)
    {
        throw new NotImplementedException();
    }

    public bool Deserialize(SpanReader reader)
    {
        if (!reader.TryReadBytes(0x100, out var unknown))
        {
            return false;
        }
        
        Unknown = unknown;
        return true;
    }
}