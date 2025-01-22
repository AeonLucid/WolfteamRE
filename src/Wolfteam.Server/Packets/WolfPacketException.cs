// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-22.

namespace Wolfteam.Server.Packets;

public class WolfPacketException : Exception
{
    public WolfPacketException(string? message) : base(message)
    {
    }

    public WolfPacketException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}