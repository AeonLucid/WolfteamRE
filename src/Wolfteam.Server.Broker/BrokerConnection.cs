// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-21.

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using Serilog;
using Wolfteam.Packets;
using Wolfteam.Packets.Broker;
using Wolfteam.Packets.Broker.Data;

namespace Wolfteam.Server.Broker;

public class BrokerConnection : WolfConnection
{
    private static readonly ILogger Logger = Log.ForContext<BrokerConnection>();
    private static readonly ClientVersion ClientVersion = ClientVersion.IS_854;
    
    public BrokerConnection(Guid id, Socket client) : base(Logger, id, client)
    {
    }

    private async ValueTask SendPacketAsync(short sequence, IWolfPacket packet)
    {
        // Calculate sizes.
        var payloadSizeExtra = 1; // ErrorCode (??)
        var payloadSize = packet.Size(ClientVersion) + payloadSizeExtra;
        var payloadSizeEnc = payloadSize == 0 ? 0 : (payloadSize + 15) & ~15;
        var packetSize = PacketHeader.Size + payloadSizeEnc;
        
        // Rent buffer.
        using var packetBuffer = MemoryPool<byte>.Shared.Rent(packetSize);
        
        var buffer = packetBuffer.Memory.Span.Slice(0, packetSize);
        
        // Create header.
        if (!PacketSerializer.TryGetId(packet, out var id))
        {
            Logger.Warning("Failed to get packet id for {Packet}", packet);
            return;    
        }
        
        var header = new PacketHeader
        {
            Random = (byte)Random.Shared.Next(0, byte.MaxValue),
            Id = (short)id,
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
        packet.Serialize(ClientVersion, ref payloadWriter);
        
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
            await SendPacketAsync(header.Sequence, new CS_BR_CHAINLIST_ACK
            {
                // Must be 14 entries.
                Chainlist = Enumerable.Repeat((byte)14, 14).ToArray()
            });
        } 
        else if (packet is CS_BR_WORLDLIST_REQ)
        {
            await SendPacketAsync(header.Sequence, new CS_BR_WORLDLIST_ACK
            {
                // 20 entries of the same server.
                Entries =
                [
                    new WorldListEntry
                    {
                        IpAddress = 0x0100007F,
                        Port = 40850,
                        Uk1 = 3,
                        Uk2 = 0,
                        Uk3 = 500,
                        PlayerCount1 = 0,
                        PlayerCount2 = 500
                    },
                    new WorldListEntry
                    {
                        IpAddress = 0x0100007F,
                        Port = 40850,
                        Uk1 = 3,
                        Uk2 = 0,
                        Uk3 = 500,
                        PlayerCount1 = 0,
                        PlayerCount2 = 500
                    },
                    new WorldListEntry
                    {
                        IpAddress = 0x0100007F,
                        Port = 40850,
                        Uk1 = 3,
                        Uk2 = 0,
                        Uk3 = 500,
                        PlayerCount1 = 0,
                        PlayerCount2 = 500
                    },
                    new WorldListEntry
                    {
                        IpAddress = 0x0100007F,
                        Port = 40850,
                        Uk1 = 3,
                        Uk2 = 0,
                        Uk3 = 500,
                        PlayerCount1 = 0,
                        PlayerCount2 = 500
                    },
                    new WorldListEntry
                    {
                        IpAddress = 0x0100007F,
                        Port = 40850,
                        Uk1 = 3,
                        Uk2 = 0,
                        Uk3 = 500,
                        PlayerCount1 = 0,
                        PlayerCount2 = 500
                    },
                    new WorldListEntry
                    {
                        IpAddress = 0x0100007F,
                        Port = 40850,
                        Uk1 = 3,
                        Uk2 = 0,
                        Uk3 = 500,
                        PlayerCount1 = 0,
                        PlayerCount2 = 500
                    },
                    new WorldListEntry
                    {
                        IpAddress = 0x0100007F,
                        Port = 40850,
                        Uk1 = 3,
                        Uk2 = 0,
                        Uk3 = 500,
                        PlayerCount1 = 0,
                        PlayerCount2 = 500
                    },
                    new WorldListEntry
                    {
                        IpAddress = 0x0100007F,
                        Port = 40850,
                        Uk1 = 3,
                        Uk2 = 0,
                        Uk3 = 500,
                        PlayerCount1 = 200,
                        PlayerCount2 = 500
                    },
                    new WorldListEntry
                    {
                        IpAddress = 0x0100007F,
                        Port = 40850,
                        Uk1 = 3,
                        Uk2 = 0,
                        Uk3 = 500,
                        PlayerCount1 = 0,
                        PlayerCount2 = 500
                    },
                    new WorldListEntry
                    {
                        IpAddress = 0x0100007F,
                        Port = 40850,
                        Uk1 = 3,
                        Uk2 = 0,
                        Uk3 = 500,
                        PlayerCount1 = 0,
                        PlayerCount2 = 500
                    },
                    new WorldListEntry
                    {
                        IpAddress = 0x0100007F,
                        Port = 40850,
                        Uk1 = 3,
                        Uk2 = 0,
                        Uk3 = 500,
                        PlayerCount1 = 0,
                        PlayerCount2 = 500
                    },
                    new WorldListEntry
                    {
                        IpAddress = 0x0100007F,
                        Port = 40850,
                        Uk1 = 3,
                        Uk2 = 0,
                        Uk3 = 500,
                        PlayerCount1 = 0,
                        PlayerCount2 = 500
                    },
                    new WorldListEntry
                    {
                        IpAddress = 0x0100007F,
                        Port = 40850,
                        Uk1 = 3,
                        Uk2 = 0,
                        Uk3 = 500,
                        PlayerCount1 = 0,
                        PlayerCount2 = 500
                    },
                    new WorldListEntry
                    {
                        IpAddress = 0x0100007F,
                        Port = 40850,
                        Uk1 = 3,
                        Uk2 = 0,
                        Uk3 = 500,
                        PlayerCount1 = 0,
                        PlayerCount2 = 500
                    },
                    new WorldListEntry
                    {
                        IpAddress = 0x0100007F,
                        Port = 40850,
                        Uk1 = 3,
                        Uk2 = 0,
                        Uk3 = 500,
                        PlayerCount1 = 0,
                        PlayerCount2 = 500
                    },
                    new WorldListEntry
                    {
                        IpAddress = 0x0100007F,
                        Port = 40850,
                        Uk1 = 3,
                        Uk2 = 0,
                        Uk3 = 500,
                        PlayerCount1 = 0,
                        PlayerCount2 = 500
                    },
                    new WorldListEntry
                    {
                        IpAddress = 0x0100007F,
                        Port = 40850,
                        Uk1 = 3,
                        Uk2 = 0,
                        Uk3 = 500,
                        PlayerCount1 = 0,
                        PlayerCount2 = 500
                    },
                    new WorldListEntry
                    {
                        IpAddress = 0x0100007F,
                        Port = 40850,
                        Uk1 = 3,
                        Uk2 = 0,
                        Uk3 = 500,
                        PlayerCount1 = 0,
                        PlayerCount2 = 500
                    },
                    new WorldListEntry
                    {
                        IpAddress = 0x0100007F,
                        Port = 40850,
                        Uk1 = 3,
                        Uk2 = 0,
                        Uk3 = 500,
                        PlayerCount1 = 0,
                        PlayerCount2 = 500
                    },
                    new WorldListEntry
                    {
                        IpAddress = 0x0100007F,
                        Port = 40850,
                        Uk1 = 3,
                        Uk2 = 0,
                        Uk3 = 500,
                        PlayerCount1 = 0,
                        PlayerCount2 = 500
                    }
                ]
            });
        }
        else if (packet is CS_BR_WORLDINFO_REQ)
        {
            await SendPacketAsync(header.Sequence, new CS_BR_WORLDINFO_ACK
            {
                WorldInfoEntries =
                [
                    new WorldInfoEntry
                    {
                        Name = "Newbie Channel",
                        Kills = 0,
                        Deaths = 10000,
                        GP = 100,
                        Gold = 100
                    },
                    new WorldInfoEntry
                    {
                        Name = "Expert Channel",
                        Kills = 0,
                        Deaths = 10000,
                        GP = 100,
                        Gold = 100
                    },
                    new WorldInfoEntry
                    {
                        Name = "Free Channel 1",
                        Kills = 0,
                        Deaths = 10000,
                        GP = 70,
                        Gold = 70
                    },
                    new WorldInfoEntry
                    {
                        Name = "Free Channel 2",
                        Kills = 0,
                        Deaths = 10000,
                        GP = 70,
                        Gold = 70
                    },
                    new WorldInfoEntry
                    {
                        Name = "Free Channel 3",
                        Kills = 0,
                        Deaths = 10000,
                        GP = 70,
                        Gold = 70
                    },
                    new WorldInfoEntry
                    {
                        Name = "Free Channel 4",
                        Kills = 0,
                        Deaths = 10000,
                        GP = 70,
                        Gold = 70
                    },
                    new WorldInfoEntry
                    {
                        Name = "Free Channel 5",
                        Kills = 0,
                        Deaths = 10000,
                        GP = 70,
                        Gold = 70
                    },
                    new WorldInfoEntry
                    {
                        Name = "Free Channel 6",
                        Kills = 0,
                        Deaths = 10000,
                        GP = 70,
                        Gold = 70
                    },
                    new WorldInfoEntry
                    {
                        Name = "Pride Battle",
                        Kills = 0,
                        Deaths = 10000,
                        GP = 100,
                        Gold = 100
                    },
                    new WorldInfoEntry
                    {
                        Name = "Pride Ladder",
                        Kills = 0,
                        Deaths = 10000,
                        GP = 0,
                        Gold = 0
                    },
                    new WorldInfoEntry
                    {
                        Name = "Van helsing",
                        Kills = 0,
                        Deaths = 10000,
                        GP = 0,
                        Gold = 0
                    },
                    new WorldInfoEntry
                    {
                        Name = "Team Destruction",
                        Kills = 0,
                        Deaths = 10000,
                        GP = 0,
                        Gold = 0
                    },
                    new WorldInfoEntry
                    {
                        Name = "Pride Battle 1",
                        Kills = 0,
                        Deaths = 10000,
                        GP = 0,
                        Gold = 0
                    },
                    new WorldInfoEntry
                    {
                        Name = "Pride Battle 2",
                        Kills = 0,
                        Deaths = 10000,
                        GP = 0,
                        Gold = 0
                    },
                    new WorldInfoEntry
                    {
                        Name = "Tester Channel 1",
                        Kills = 0,
                        Deaths = 10000,
                        GP = 0,
                        Gold = 0
                    },
                    new WorldInfoEntry
                    {
                        Name = "Tester Channel 2",
                        Kills = 0,
                        Deaths = 10000,
                        GP = 0,
                        Gold = 0
                    },
                    new WorldInfoEntry
                    {
                        Name = "empty",
                        Kills = 0,
                        Deaths = 10000,
                        GP = 0,
                        Gold = 0
                    },
                    new WorldInfoEntry
                    {
                        Name = "empty",
                        Kills = 0,
                        Deaths = 10000,
                        GP = 0,
                        Gold = 0
                    },
                    new WorldInfoEntry
                    {
                        Name = "empty",
                        Kills = 0,
                        Deaths = 10000,
                        GP = 0,
                        Gold = 0
                    },
                    new WorldInfoEntry
                    {
                        Name = "empty",
                        Kills = 0,
                        Deaths = 10000,
                        GP = 0,
                        Gold = 0
                    }
                ]
            });
        }
        else if (packet is CS_BR_RELAYLIST_REQ)
        {
            await SendPacketAsync(header.Sequence, new CS_BR_RELAYLIST_ACK
            {
                // Seems to be an outdated length field.
                Padding = 20,
                // 20 entries of the same relay.
                Relays = Enumerable.Range(0, 20).Select(id => new RelayEntry
                {
                    Id = (byte)id,
                    Address = 0x0100007F,
                    Port = 16540,
                    Padding = 0
                }).ToArray()
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
        var packetId = (PacketId)header.Id;
        
        if (!PacketSerializer.TryDeserialize(packetId, ClientVersion, payload, out packet))
        {
            Logger.Warning("Failed to deserialize packet id {PacketId}, data '{Data}'", packetId, Convert.ToHexStringLower(payload));
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