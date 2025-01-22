﻿// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-21.

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Security.Cryptography;
using Serilog;
using Wolfteam.Server.Packets;

namespace Wolfteam.Server.Broker;

public class BrokerConnection : WolfConnection
{
    private static readonly ILogger Logger = Log.ForContext<BrokerConnection>();
    
    public BrokerConnection(Guid id, Socket client) : base(Logger, id, client)
    {
    }// BA 02 12 00 00

    protected override ValueTask ProcessPacketAsync(ReadOnlySequence<byte> buffer)
    {
        Logger.Debug("Processing packet {Packet}", Convert.ToHexStringLower(buffer.ToArray()));

        if (!TryParsePacket(buffer, out var header, out var packet))
        {
            Logger.Warning("Failed to parse packet");
            return ValueTask.CompletedTask;
        }
        
        Logger.Debug("Id       {PacketId}", header.Id);
        Logger.Debug("Sequence {Sequence}", header.Sequence);
        Logger.Debug("Payload  {@Payload}", packet);
        
        return ValueTask.CompletedTask;
    }

    /// <summary>
    ///     Parses the full encrypted packet.
    /// </summary>
    private bool TryParsePacket(ReadOnlySequence<byte> buffer, out PacketHeader header, [NotNullWhen(true)] out IWolfPacket? packet)
    {
        // Read header.
        Span<byte> packetKey = stackalloc byte[16];
        
        if (!TryParseHeader(ref buffer, packetKey, out header))
        {
            packet = null;
            return false;
        }
        
        // Read body.
        var lenPayload = header.Blocks * 16;
        var lenPacket = lenPayload + 8;
        if (lenPacket > buffer.Length)
        {
            packet = null;
            return false;
        }
        
        // Read payload.
        var payload = lenPayload == 0 
            ? Span<byte>.Empty 
            : lenPayload > 256 
                ? new byte[lenPayload] 
                : stackalloc byte[lenPayload];

        if (!payload.IsEmpty)
        {
            buffer.Slice(8, lenPayload).CopyTo(payload);

            // Decrypt payload.
            using var aes = Aes.Create();
            
            aes.Key = packetKey.ToArray();
            
            if (!aes.TryDecryptEcb(payload, payload, PaddingMode.None, out var written) || written != lenPayload)
            {
                Logger.Warning("Failed to decrypt packet payload");
                packet = null;
                return false;
            }
        }
        
        // Parse payload.
        if (!PacketSerializer.TryDeserialize(header, payload, out packet))
        {
            Logger.Warning("Failed to deserialize packet id {PacketId}, data '{Data}'", header.Id, Convert.ToHexStringLower(payload));
            packet = null;
            return false;
        }
        
        return true;
    }

    /// <summary>
    ///     Parses the header of an encrypted packet.
    /// </summary>
    private bool TryParseHeader(ref ReadOnlySequence<byte> buffer, Span<byte> key, out PacketHeader header)
    {
        Span<byte> plain = stackalloc byte[8];
        
        // Read potential packet header.
        if (buffer.Length < 8)
        {
            header = default;
            return false;
        }
        
        buffer.Slice(0, 8).CopyTo(plain);
        
        // Decrypt the packet header.
        if (!PacketCrypto.TryDecryptHeader(plain, key))
        {
            header = default;
            return false;
        }
        
        // Parse the header.
        header = PacketCrypto.ReadHeader(plain);
        return true;
    }

    protected override bool TryReadPacket(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> packet)
    {
        // Read potential packet header.
        Span<byte> packetKey = stackalloc byte[16];
        
        if (!TryParseHeader(ref buffer, packetKey, out var header))
        {
            packet = default;
            return false;
        }
        
        // Check if we have the full packet.
        var lenPayload = header.Blocks * 16;
        var lenPacket = lenPayload + 8;
        if (lenPacket > buffer.Length)
        {
            packet = default;
            return false;
        }
        
        packet = buffer.Slice(buffer.Start, lenPacket);
        buffer = buffer.Slice(packet.End);
        return true;
    }
}