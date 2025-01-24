// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-22.

namespace Wolfteam.Packets;

public interface IWolfPacket
{
    int Size(ClientVersion version);
    void Serialize(ClientVersion version, ref SpanWriter writer);
    bool Deserialize(ClientVersion version, ref SpanReader reader);
}