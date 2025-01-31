// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-21.

using Wolfteam.Server;

namespace Wolfteam.Tests.Crypto;

public class PacketCryptoTests
{
    [Test]
    public void DecryptPacket()
    {
        var packet = Convert.FromHexString("66b732a7223762ee5c086db87a2c54bd4e6dc92ee96c7d3d").AsSpan();
        var key = new byte[16];

        if (!PacketCrypto.TryDecryptHeader(packet.Slice(0, 8), key))
        {
            Assert.Fail("Failed to decrypt header.");
            return;
        }
        
        if (!PacketCrypto.TryDecryptPayload(key, packet.Slice(8)))
        {
            Assert.Fail("Failed to decrypt payload.");
            return;
        }

        var header = PacketHeader.Deserialize(packet);
        var payload = packet.Slice(8);
        
        // Full packet.
        Assert.That(Convert.ToHexStringLower(packet), Is.EqualTo("b8021100000100cc03616263000000000000000000000000"));
        
        // Header properties.
        Assert.Multiple(() =>
        {
            Assert.That(header.Random, Is.EqualTo(0xb8), "Random is incorrect.");
            Assert.That(header.Id, Is.EqualTo(0x1102), "Id is incorrect.");
            Assert.That(header.Sequence, Is.EqualTo(0x0000), "Sequence is incorrect.");
            Assert.That(header.Blocks, Is.EqualTo(0x0001), "Blocks is incorrect.");
            Assert.That(header.Checksum, Is.EqualTo(0xcc), "Checksum is incorrect.");
        });
    }
    
    [Test]
    public void EncryptPacket()
    {
        Span<byte> packet = stackalloc byte[24];
        
        var key = new byte[16];
        var header = new PacketHeader
        {
            Random = 0xb8,
            Id = 0x1102,
            Sequence = 0x0000,
            Blocks = 0x0001,
            Checksum = 0x00
        };
        
        // Write header to packet.
        header.Serialize(packet);
        
        // Header.
        Assert.That(Convert.ToHexStringLower(packet.Slice(0, 8)), Is.EqualTo("b8021100000100cc"), "Header serialized incorrectly.");
        
        // Write payload.
        packet[8] = 0x03;
        packet[9] = 0x61;
        packet[10] = 0x62;
        packet[11] = 0x63;
        
        if (!PacketCrypto.TryEncryptHeader(packet.Slice(0, 8), key))
        {
            Assert.Fail("Failed to encrypt header.");
            return;
        }
        
        if (!PacketCrypto.TryEncryptPayload(key, packet.Slice(8)))
        {
            Assert.Fail("Failed to encrypt payload.");
            return;
        }
        
        // Header.
        Assert.That(Convert.ToHexStringLower(packet.Slice(0, 8)), Is.EqualTo("66b732a7223762ee"), "Header encrypted incorrectly.");
        
        // Payload.
        Assert.That(Convert.ToHexStringLower(packet.Slice(8)), Is.EqualTo("5c086db87a2c54bd4e6dc92ee96c7d3d"), "Payload encrypted incorrectly.");
    }
}