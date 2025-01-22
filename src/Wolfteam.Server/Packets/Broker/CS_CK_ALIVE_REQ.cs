// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-22.

using Wolfteam.Server.Buffers;

namespace Wolfteam.Server.Packets.Broker;

public class CS_CK_ALIVE_REQ : IWolfPacket
{
    public int CalculateSize()
    {
        return 0;
    }

    public void Serialize(ref SpanWriter writer)
    {
    }

    public bool Deserialize(ref SpanReader reader)
    {
        return true;
    }
}