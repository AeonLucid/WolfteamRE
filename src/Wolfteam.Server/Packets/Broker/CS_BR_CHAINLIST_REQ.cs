// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-22.

using System.Text;
using Wolfteam.Server.Buffers;

namespace Wolfteam.Server.Packets.Broker;

public class CS_BR_CHAINLIST_REQ : IWolfPacket
{
    public string? Username { get; set; }
    
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
        if (!reader.TryReadU8(out var usernameLen))
        {
            return false;
        }
        
        if (!reader.TryReadString(Encoding.UTF8, usernameLen, out var username))
        {
            return false;
        }
        
        Username = username;
        return true;
    }
}