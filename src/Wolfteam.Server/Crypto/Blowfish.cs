// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-21.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace Wolfteam.Server.Crypto;

/// <summary>
/// Blowfish context for encrypting and decrypting data.
/// </summary>
public class Blowfish
{
    private readonly uint[] _p;
    private readonly uint[,] _s;
    
    /// <summary>
    /// Initializes a BlowfishContext instance.
    /// </summary>
    public Blowfish()
    {
        _p = BlowfishTable.P;
        _s = BlowfishTable.S;
    }

    /// <summary>
    /// Encrypts a span of bytes in place.
    /// </summary>
    /// <param name="data">The span to encrypt.</param>
    public void Encrypt(Span<byte> data)
    {
        if (data.Length % 8 != 0)
        {
            throw new Exception("Length must be a multiple of 8");
        }

        for (var i = 0; i < data.Length; i += 8)
        {
            // Encode data in 8-byte blocks.
            var xl = BinaryPrimitives.ReadUInt32LittleEndian(data);
            var xr = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(4));
            Encrypt(ref xl, ref xr);
            
            // Replace data.
            BinaryPrimitives.WriteUInt32LittleEndian(data, xl);
            BinaryPrimitives.WriteUInt32LittleEndian(data.Slice(4), xr);
        }
    }

    /// <summary>
    ///     Decrypts a span of bytes in place.
    /// </summary>
    /// <param name="data">The data to decrypt.</param>
    /// <exception cref="Exception"></exception>
    public void Decrypt(Span<byte> data)
    {
        if (data.Length % 8 != 0)
        {
            throw new Exception("Length must be a multiple of 8");
        }

        for (var i = 0; i < data.Length; i += 8)
        {
            // Decode data in 8-byte blocks.
            var xl = BinaryPrimitives.ReadUInt32LittleEndian(data);
            var xr = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(4));
            Decrypt(ref xl, ref xr);
            
            // Replace data.
            BinaryPrimitives.WriteUInt32LittleEndian(data, xl);
            BinaryPrimitives.WriteUInt32LittleEndian(data.Slice(4), xr);
        }
    }

    private void Encrypt(ref uint datal, ref uint datar)
    {
        uint xl = datal, xr = datar, temp;
        short i;

        for (i = 0; i < 16; ++i)
        {
            xl ^= _p[i];
            xr = Loword(xl) ^ xr;

            // Exchange xl and xr
            temp = xl;
            xl = xr;
            xr = temp;
        }

        // Exchange xl and xr
        temp = xl;
        xl = xr;
        xr = temp;

        xl ^= _p[17];
        xr ^= _p[16];

        datal = xl;
        datar = xr;
    }

    private void Decrypt(ref uint datal, ref uint datar)
    {
        uint xl = datal, xr = datar, temp;
        short i;

        for (i = 17; i > 1; --i)
        {
            xl ^= _p[i];
            xr = Loword(xl) ^ xr;
            
            // Exchange xl and xr
            temp = xl;
            xl = xr;
            xr = temp;
        }
        
        // Exchange xl and xr
        temp = xl;
        xl = xr;
        xr = temp;

        xl ^= _p[0];
        xr ^= _p[1];

        datal = xl;
        datar = xr;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint Loword(uint number)
    {
        var a = (byte)(number & 0xFF);
        var b = (byte)((number >> 8) & 0xFF);
        var c = (byte)((number >> 16) & 0xFF);
        var d = (byte)((number >> 24) & 0xFF);
        
        return (_s[3, a] + _s[2, b]) ^
               (_s[1, c] + _s[0, d]);
    }
}