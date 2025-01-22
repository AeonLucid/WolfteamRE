// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-22.

using System.Diagnostics.CodeAnalysis;
using Serilog;
using Wolfteam.Server.Buffers;
using Wolfteam.Server.Packets.Broker;

namespace Wolfteam.Server.Packets;

public static class PacketSerializer
{
    private static readonly ILogger Logger = Log.ForContext(typeof(PacketSerializer));
    
    public static bool TryDeserialize(PacketHeader header, ReadOnlySpan<byte> payload, [NotNullWhen(true)] out IWolfPacket? packet)
    {
        // TODO: This is a temporary solution to deserialize packets.
        // In the future, we should use a more efficient method than a switch.
        packet = header.Id switch
        {
            0x1100 => new CS_BR_WORLDLIST_REQ(),
            0x1102 => new CS_BR_CHAINLIST_REQ(),
            0x1104 => new CS_BR_RELAYLIST_REQ(),
            0x1106 => new CS_BR_WORLDINFO_REQ(),
            0x1202 => new CS_CK_ALIVE_REQ(),
            0x1312 => new CS_CH_DISCONNECT_REQ(),
            _ => null
        };

        if (packet == null)
        {
            Logger.Warning("Missing class for packet id {PacketId}", header.Id);
            Logger.Verbose("Tip: find with \"{A:x2} {B:x2} 00 00\"", header.Id & 0xFF, header.Id >> 8);
            return false;
        }

        var reader = new SpanReader(payload);
        return packet.Deserialize(ref reader);
    }
}