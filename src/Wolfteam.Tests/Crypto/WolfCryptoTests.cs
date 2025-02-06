// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-02-06.

using System.Security.Cryptography;
using Wolfteam.Server.Crypto;

namespace Wolfteam.Tests.Crypto;

public class WolfCryptoTests
{
    [Test]
    public void TestLoginServer()
    {
        var passwordHash = MD5.HashData("qqq"u8.ToArray());
        var passwordHashHex = Convert.ToHexStringLower(passwordHash.AsSpan(6, 10));
        
        var key = WolfCrypto.CreateAuthKey("AeonLucid", passwordHashHex, 0x29);
        var expect = new byte[] { 0xac, 0x0e, 0x8b, 0x8f, 0xac, 0xa1, 0x75, 0x82, 0x8d, 0x7f, 0x7a, 0x90, 0x72, 0x39, 0x70, 0x7a };
        
        Assert.That(key, Is.EqualTo(expect));
    }
    
    [Test]
    public void TestBuddyServer()
    {
        var key = WolfCrypto.CreateAuthKey("AeonLucid", null, 0x78df);
        var expect = new byte[] { 0x1f, 0x20, 0xde, 0xfa, 0x44, 0xb5, 0x5d, 0x56, 0x85, 0xd4, 0xf8, 0x52, 0x55, 0x67, 0x5c, 0xd8 };

        Assert.That(key, Is.EqualTo(expect));
    }
}