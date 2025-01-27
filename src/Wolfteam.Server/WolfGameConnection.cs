// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-24.

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using Serilog;
using Wolfteam.Packets;

namespace Wolfteam.Server;

public abstract class WolfGameConnection : WolfConnection
{
    private readonly ILogger _logger;
    private readonly ClientVersion _clientVersion;

    private int _sequence;

    protected WolfGameConnection(ILogger logger, ClientVersion clientVersion, Guid id, Socket client) : base(logger, id, client)
    {
        _logger = logger;
        _clientVersion = clientVersion;
    }

    public abstract ValueTask HandlePacketAsync(PacketId id, IWolfPacket packet);
    
    protected async ValueTask SendPacketAsync(IWolfPacket packet)
    {
        // Calculate sizes.
        var payloadSizeExtra = 1; // ErrorCode (??)
        var payloadSize = packet.Size(_clientVersion) + payloadSizeExtra;
        var payloadSizeEnc = payloadSize == 0 ? 0 : (payloadSize + 15) & ~15;
        var packetSize = PacketHeader.Size + payloadSizeEnc;
        
        // Rent buffer.
        using var packetBuffer = MemoryPool<byte>.Shared.Rent(packetSize);
        
        var buffer = packetBuffer.Memory.Span.Slice(0, packetSize);
        
        // Create header.
        if (!PacketSerializer.TryGetId(packet, out var id))
        {
            _logger.Warning("Failed to get packet id for {Packet}", packet);
            return;    
        }
        
        var sequence = Interlocked.Exchange(ref _sequence, _sequence + 1);
        if (sequence >= short.MaxValue)
        {
            throw new InvalidOperationException("Sequence overflow, unhandled case");    
        }
        
        var header = new PacketHeader
        {
            Random = (byte)Random.Shared.Next(0, byte.MaxValue),
            Id = (short)id,
            Sequence = (short)sequence,
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
        packet.Serialize(_clientVersion, ref payloadWriter);
        
        // Zero out remaining payload bytes.
        var payloadRemaining = payloadSizeEnc - payloadSize;
        if (payloadRemaining > 0)
        {
            buffer.Slice(PacketHeader.Size + payloadSize, payloadRemaining).Clear();
        }
    
        _logger.Debug("Sending encrypted packet {PacketId} {Packet}", id, Convert.ToHexStringLower(buffer.ToArray()));
        
        // Encrypt header.
        Span<byte> key = stackalloc byte[16];
        
        if (!PacketCrypto.TryEncryptHeader(buffer.Slice(0, PacketHeader.Size), key))
        {
            _logger.Warning("Failed to encrypt packet header");
            return;
        }
    
        // Encrypt payload.
        if (!PacketCrypto.TryEncryptPayload(key, buffer.Slice(PacketHeader.Size)))
        {
            _logger.Warning("Failed to encrypt packet payload");
            return;
        }
        
        // Send packet.
        await WriteDataAsync(packetBuffer.Memory.Slice(0, packetSize));
    }

    protected override async ValueTask ProcessPacketAsync(ReadOnlySequence<byte> buffer)
    {
        _logger.Verbose("Processing packet {Packet}", Convert.ToHexStringLower(buffer.ToArray()));

        if (!TryParsePacket(buffer, out var header, out var packet))
        {
            _logger.Warning("Failed to parse packet");
            return;
        }
        
        _logger.Debug("Id       {PacketId}", header.Id);
        _logger.Debug("Sequence {Sequence}", header.Sequence);
        _logger.Debug("Payload  {@Payload}", packet);

        await HandlePacketAsync((PacketId)header.Id, packet);
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
                _logger.Warning("Failed to decrypt packet payload");
                packet = null;
                return false;
            }
        }
        
        // Parse payload.
        _logger.Debug("Deserializing payload {Payload}", Convert.ToHexStringLower(payload));
        
        var packetId = (PacketId)header.Id;
        
        if (!PacketSerializer.TryDeserialize(packetId, _clientVersion, payload, out packet))
        {
            _logger.Warning("Failed to deserialize packet id {PacketId}, data '{Data}'. Got {@WolfPacket}", packetId, Convert.ToHexStringLower(payload), packet);
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