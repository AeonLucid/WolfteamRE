// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-21.

namespace Wolfteam.Server;

public class PacketReaderException : Exception
{
    public PacketReaderException(string? message) : base(message)
    {
    }

    public PacketReaderException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}