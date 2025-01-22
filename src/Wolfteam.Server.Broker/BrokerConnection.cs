// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-21.

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using Serilog;
using Wolfteam.Server.Buffers;
using Wolfteam.Server.Packets;
using Wolfteam.Server.Packets.Broker;

namespace Wolfteam.Server.Broker;

public class BrokerConnection : WolfConnection
{
    private static readonly ILogger Logger = Log.ForContext<BrokerConnection>();
    
    public BrokerConnection(Guid id, Socket client) : base(Logger, id, client)
    {
    }

    private async ValueTask SendPacketAsync(short id, short sequence, IWolfPacket packet)
    {
        // Calculate sizes.
        var payloadSizeExtra = 1; // ErrorCode (??)
        var payloadSize = packet.CalculateSize() + payloadSizeExtra;
        var payloadSizeEnc = payloadSize == 0 ? 0 : (payloadSize + 15) & ~15;
        var packetSize = PacketHeader.Size + payloadSizeEnc;
        
        // Rent buffer.
        using var packetBuffer = MemoryPool<byte>.Shared.Rent(packetSize);
        
        var buffer = packetBuffer.Memory.Span.Slice(0, packetSize);
        
        // Create header.
        var header = new PacketHeader
        {
            Random = (byte)Random.Shared.Next(0, byte.MaxValue),
            Id = id,
            Sequence = sequence,
            Blocks = (short)(payloadSizeEnc >> 4),
            Checksum = 0
        };
        
        // Write header.
        header.Serialize(buffer);
        
        // Write payload.
        var payloadWriter = new SpanWriter(buffer.Slice(PacketHeader.Size, payloadSize));
        
        // - error code
        payloadWriter.WriteU8(0);
        
        // - actual payload
        packet.Serialize(ref payloadWriter);
        
        // Zero out remaining payload bytes.
        var payloadRemaining = payloadSizeEnc - payloadSize;
        if (payloadRemaining > 0)
        {
            buffer.Slice(PacketHeader.Size + payloadSize, payloadRemaining).Clear();
        }
    
        // Encrypt header.
        Span<byte> key = stackalloc byte[16];
        
        if (!PacketCrypto.TryEncryptHeader(buffer.Slice(0, PacketHeader.Size), key))
        {
            Logger.Warning("Failed to encrypt packet header");
            return;
        }
    
        if (!PacketCrypto.TryEncryptPayload(key, buffer.Slice(PacketHeader.Size)))
        {
            Logger.Warning("Failed to encrypt packet payload");
            return;
        }
        
        // Send packet.
        await WriteDataAsync(packetBuffer.Memory.Slice(0, packetSize));
    }

    protected override async ValueTask ProcessPacketAsync(ReadOnlySequence<byte> buffer)
    {
        Logger.Debug("Processing packet {Packet}", Convert.ToHexStringLower(buffer.ToArray()));

        if (!TryParsePacket(buffer, out var header, out var packet))
        {
            Logger.Warning("Failed to parse packet");
            return;
        }
        
        Logger.Debug("Id       {PacketId}", header.Id);
        Logger.Debug("Sequence {Sequence}", header.Sequence);
        Logger.Debug("Payload  {@Payload}", packet);

        if (packet is CS_BR_CHAINLIST_REQ)
        {
            await SendPacketAsync(0x1103, header.Sequence, new CS_BR_CHAINLIST_ACK
            {
                Chainlist =
                [
                    14,
                    14,
                    14,
                    14,
                    14,
                    14,
                    14,
                    14,
                    14,
                    14,
                    14,
                    14,
                    14,
                    14
                ]
            });
        } 
        else if (packet is CS_BR_WORLDLIST_REQ)
        {
            await SendPacketAsync(0x1101, header.Sequence, new CS_BR_WORLDLIST_ACK
            {

            });
        }
        else if (packet is CS_BR_WORLDINFO_REQ)
        {
            await SendPacketAsync(0x1107, header.Sequence, new CS_BR_WORLDINFO_ACK
            {

            });
        }
        else if (packet is CS_BR_RELAYLIST_REQ)
        {
            await SendPacketAsync(0x1105, header.Sequence, new CS_BR_RELAYLIST_ACK
            {
                
            });
        }
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

            if (!PacketCrypto.TryDecryptPayload(packetKey, payload))
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
    private static bool TryParseHeader(ref ReadOnlySequence<byte> buffer, Span<byte> key, out PacketHeader header)
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
        header = PacketHeader.Deserialize(plain);
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