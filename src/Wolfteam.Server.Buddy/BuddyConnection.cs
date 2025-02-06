// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-02-05.

using System.Buffers;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Security.Cryptography;
using Serilog;
using Wolfteam.Server.Buddy.Packets;
using Wolfteam.Server.Crypto;

namespace Wolfteam.Server.Buddy;

public class BuddyConnection : WolfConnection
{
    private static readonly ILogger Logger = Log.ForContext<BuddyConnection>();
    
    public BuddyConnection(Guid id, Socket client) : base(Logger, id, client)
    {
    }

    private async Task SendPacketAsync(ushort packetId, byte[] data)
    {
        var packetLen = (ushort) (data.Length + 4);
        var packet = new byte[packetLen];
        
        BinaryPrimitives.WriteUInt16LittleEndian(packet.AsSpan(0, 2), packetLen);
        BinaryPrimitives.WriteUInt16LittleEndian(packet.AsSpan(2, 2), packetId);
        
        Buffer.BlockCopy(data, 0, packet, 4, data.Length);
        
        await WriteDataAsync(packet);
    }

    protected override async ValueTask ProcessPacketAsync(ReadOnlySequence<byte> packet)
    {
        Logger.Debug("Received packet {Packet}", Convert.ToHexStringLower(packet.ToArray()));

        var reader = new PacketReader(packet);

        var packetLen = reader.ReadShort();
        var packetId = reader.ReadShort();
        
        Logger.Debug("Packet 0x{PacketId:X} {PacketLen}", packetId, packetLen);

        switch (packetId)
        {
            // PRECREDENTIAL
            case 0x1000:
            {
                await SendPacketAsync(0x1001, [
                    0xDF, 0x78, 0x00, 0x00, // ?
                    0xFF, 0xFF, 0x07, 0x00, // ?
                ]);
                break;
            }

            // LOGIN
            case 0x1010:
            {
                using var _cryptoStatic = Aes.Create();
                using var _cryptoAuth = Aes.Create();
                
                _cryptoStatic.Key = Constants.StaticKey;
                
                var staticData = reader.ReadBytes(64);
                var authData = reader.ReadBytes(176);
                
                if (!LOGIN_REQ.TryParseStatic(_cryptoStatic, ref staticData, out var username, out var magic))
                {
                    Logger.Warning("Invalid username data");
                    return;
                }
                
                _cryptoAuth.Key = WolfCrypto.CreateAuthKey(username, null, magic);

                if (!LOGIN_REQ.TryDecryptAuth(_cryptoAuth, ref authData, out var loginOut))
                {
                    Logger.Warning("Invalid password data");
                    return;
                }

                if (loginOut.Magic != 27)
                {
                    Logger.Warning("Invalid magic {Magic}", loginOut.Magic);
                    return;
                }

                await SendPacketAsync(0x1012, Convert.FromHexString("0100313832333100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000006b00690072006100340036003200000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000040000000000000000000000000000000300010000000000000000000000000000000000000000000000000000000000"));
                
                await SendPacketAsync(0x1013, Convert.FromHexString("0000"));
                
                await SendPacketAsync(0x1011, [
                    0x00, 0x00, // Error Code?
                    0x00, 0x11, 0x22, 0x00 // ?
                ]);
                
                // NTF_VIP_IPPORT (ip uint, port short BE)
                // await SendPacketAsync(0x101F, Convert.FromHexString(""));

                // NTF_USER_STATE
                await SendPacketAsync(0x3FFF, Convert.FromHexString("01003138323331002030302035322030302036392030302037362030302000312045452045412036392041432032372032432033392000"));
                break;
            }
            
            // GROUP_GETLIST
            case 0x3150:
                await SendPacketAsync(0x3151, Convert.FromHexString("000003000100440065006600610075006c00740000000000000000000000000000000000000000000000000000000100020043006c0061006e0000006c00740000000000000000000000000000000000000000000000000000000200030052006900760061006c000000740000000000000000000000000000000000000000000000000000000300"));
                
                // await SendPacketAsync(0x3151, [
                //     0x00, 0x00, // Error Code?
                //     0x01, 0x00, // Count
                //     // Every entry is 44 bytes.
                //     0x00, 0x00, // ?
                //     0x54, 0x65, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Test
                //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                //     0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                //     0x63, 0x00, // ?
                // ]);
                break;
        }
    }

    protected override bool TryReadPacket(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> packet)
    {
        var reader = new SequenceReader<byte>(buffer);
    
        if (!reader.TryReadLittleEndian(out short packetLen))
        {
            packet = default;
            return false;
        }
    
        // Check if buffer has enough data.
        // The packet length includes the length itself.
        if (buffer.Length < packetLen)
        {
            packet = default;
            return false;
        }

        packet = buffer.Slice(buffer.Start, packetLen);
        buffer = buffer.Slice(packet.End);
        return true;
    }
}