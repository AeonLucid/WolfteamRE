// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-21.

namespace Wolfteam.Server;

public class PacketCryptoException : Exception
{
    public PacketCryptoException(string? message) : base(message)
    {
    }

    public PacketCryptoException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}