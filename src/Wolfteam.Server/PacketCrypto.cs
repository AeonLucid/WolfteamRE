// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-21.

using System.Security.Cryptography;
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
        if (header.Length != PacketHeader.Size)
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
        return header[7] == CalculateChecksum(header);
    }

    /// <summary>
    ///     Decrypts and validates the packet header.
    /// </summary>
    /// <param name="header">The incoming packet header.</param>
    /// <param name="key">Output buffer for the aes key for the payload.</param>
    /// <returns>Whether the encryption succeeded.</returns>
    public static bool TryEncryptHeader(Span<byte> header, Span<byte> key)
    {
        if (header.Length != PacketHeader.Size)
        {
            throw new PacketCryptoException("Invalid header length, must be 8.");
        }
        
        if (key.Length != 16)
        {
            throw new PacketCryptoException("Invalid key length, must be 16.");
        }
        
        // Calculate checksum.
        header[7] = CalculateChecksum(header);
        
        // Xor.
        header.CopyTo(key.Slice(8));
        
        for (var i = 1; i < 8; i++)
        {
            header[i] ^= (byte)~(2 * header[i - 1]);
        }
        
        // Blowfish.
        header.CopyTo(key.Slice(0));
        
        Blowfish.Encrypt(header);
        
        return true;
    }

    public static bool TryDecryptPayload(ReadOnlySpan<byte> key, Span<byte> payload)
    {
        using var aes = Aes.Create();
            
        aes.Key = key.ToArray();

        return aes.TryDecryptEcb(payload, payload, PaddingMode.None, out var written) || 
               written != payload.Length;
    }
    
    public static bool TryEncryptPayload(ReadOnlySpan<byte> key, Span<byte> payload)
    {
        using var aes = Aes.Create();
            
        aes.Key = key.ToArray();

        return aes.TryEncryptEcb(payload, payload, PaddingMode.None, out var written) || 
               written != payload.Length;
    }
    
    public static byte CalculateChecksum(ReadOnlySpan<byte> header)
    {
        if (header.Length != PacketHeader.Size)
        {
            throw new PacketCryptoException("Invalid header length, must be 8.");
        }
        
        byte checksum = 0;

        for (var i = 0; i < PacketHeader.Size - 1; i++)
        {
            checksum += header[i];
        }

        return checksum;
    }
}