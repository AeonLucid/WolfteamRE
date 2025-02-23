﻿// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-21.

namespace Wolfteam.Server.Login;

public static class Constants
{
    public static readonly byte[] StaticKey = [0xa9, 0x27, 0x53, 0x04, 0x1b, 0xfc, 0xac, 0xe6, 0x5b, 0x23, 0x38, 0x34, 0x68, 0x46, 0x03, 0x8c];

    public const uint PacketMagic = 0xA8C2F5D3;
}