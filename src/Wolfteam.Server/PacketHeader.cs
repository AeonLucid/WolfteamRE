// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-22.

namespace Wolfteam.Server;

public readonly struct PacketHeader
{
    public byte Random { get; init; }
    public short Id { get; init; }
    public short Sequence { get; init; }
    public short Blocks { get; init; }
    public byte Checksum { get; init; }
}