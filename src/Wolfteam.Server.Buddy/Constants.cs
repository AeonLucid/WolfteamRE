// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-02-05.

namespace Wolfteam.Server.Buddy;

internal static class Constants
{
    public static readonly byte[] StaticKey = [0x2c, 0x45, 0x92, 0x6c, 0xf3, 0x39, 0x66, 0x42, 0xb6, 0x70, 0xd0, 0x06, 0xa1, 0xfa, 0x81, 0x82];

    // TODO: Maybe swap endian
    public const uint PacketMagic = 0x2dbabe65;
}