// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-21.

using System.Security.Cryptography;
using Wolfteam.Server.Crypto;

namespace Wolfteam.Server.Tests.Crypto;

public class PacketCryptoTests
{
    [Test]
    public void DecryptPacket()
    {
        var packet = Convert.FromHexString("66b732a7223762ee5c086db87a2c54bd4e6dc92ee96c7d3d").AsSpan();
        var header = packet.Slice(0, 8);
        var aesKey = new byte[16];
        var aesKeySpan = aesKey.AsSpan();
        
        // Blowfish.
        var blow = new Blowfish();
        
        blow.Decrypt(header);
        
        header.CopyTo(aesKeySpan.Slice(0));
        
        // Xor.
        for (var i = 7; i > 0; i--)
        {
            header[i] ^= (byte)~(2 * header[i - 1]);
        }
        
        header.CopyTo(aesKeySpan.Slice(8));
        
        // AES.
        var aes = Aes.Create();

        aes.Key = aesKey;
        aes.DecryptEcb(packet.Slice(8), packet.Slice(8), PaddingMode.None);
        
        // Output.
        Assert.That(Convert.ToHexStringLower(packet), Is.EqualTo("b8021100000100cc03616263000000000000000000000000"));
    }
}