// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-21.

using System.Buffers.Binary;
using Wolfteam.Server.Crypto;

namespace Wolfteam.Server;

public static class PacketCrypto
{
    private static readonly Blowfish Blowfish = new();
    
    /// <summary>
    ///     Decrypts and validates the packet header.
    /// </summary>
    /// <param name="header">The incoming packet header.</param>
    /// <param name="key">Output buffer for the aes key for the payload.</param>
    /// <returns>Whether the packet header is valid.</returns>
    public static bool TryDecryptHeader(Span<byte> header, Span<byte> key)
    {
        if (header.Length != 8)
        {
            throw new PacketCryptoException("Invalid header length, must be 8.");
        }

        if (key.Length != 16)
        {
            throw new PacketCryptoException("Invalid key length, must be 16.");
        }
        
        // Blowfish.
        Blowfish.Decrypt(header);
        
        header.CopyTo(key.Slice(0));
        
        // Xor.
        for (var i = 7; i > 0; i--)
        {
            header[i] ^= (byte)~(2 * header[i - 1]);
        }
        
        header.CopyTo(key.Slice(8));
        
        // Validate checksum.
        var checksumPacket = header[7];
        byte checksumOur = 0;

        for (var i = 0; i < 7; i++)
        {
            checksumOur += header[i];
        }
        
        return checksumOur == checksumPacket;
    }

    public static PacketHeader ReadHeader(ReadOnlySpan<byte> header)
    {
        if (header.Length != 8)
        {
            throw new PacketCryptoException("Invalid header length, must be 8.");
        }
        
        return new PacketHeader
        {
            Random = header[0],
            Id = BinaryPrimitives.ReadInt16LittleEndian(header.Slice(1)),
            Sequence = BinaryPrimitives.ReadInt16LittleEndian(header.Slice(3)),
            Blocks = BinaryPrimitives.ReadInt16LittleEndian(header.Slice(5)),
            Checksum = header[7]
        };
    }
}