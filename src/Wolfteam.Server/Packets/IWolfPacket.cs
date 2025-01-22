// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-22.

using Wolfteam.Server.Buffers;

namespace Wolfteam.Server.Packets;

public interface IWolfPacket
{
    int CalculateSize();
    void Serialize(ref SpanWriter writer);
    bool Deserialize(ref SpanReader reader);
}