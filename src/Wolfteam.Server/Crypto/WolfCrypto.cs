// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-02-06.

using System.Buffers;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Wolfteam.Server.Crypto;

public static class WolfCrypto
{
    public static byte[] CreateAuthKey(string username, string? password, uint magic)
    {
        // Get lengths.
        var lenUsername = Encoding.UTF8.GetByteCount(username);
        var lenPassword = password != null ? Encoding.UTF8.GetByteCount(password) : 0;
        const int lenMagic = sizeof(uint);

        // Allocate enough space for hash input.
        var hashLen = lenUsername + lenPassword + lenMagic;
        var hashInput = hashLen > 256 ? new byte[hashLen] : stackalloc byte[hashLen];

        // Copy username.
        Encoding.UTF8.GetBytes(username, hashInput);

        // Copy password.
        if (password != null)
        {
            Encoding.UTF8.GetBytes(password, hashInput.Slice(lenUsername));
        }

        // Copy magic.
        BinaryPrimitives.WriteUInt32LittleEndian(hashInput.Slice(lenUsername + lenPassword), magic);

        // Hash input.        
        Span<byte> hashOutput = stackalloc byte[20];
        WolfSHA1.Hash(hashInput, hashOutput);

        // Setup aes key.
        return hashOutput.Slice(0, 16).ToArray();
    }

    public static bool TryReadEncrypted(Aes crypto, scoped Span<byte> buffer, ref ReadOnlySequence<byte> reader)
    {
        reader.CopyTo(buffer);
        return crypto.TryDecryptEcb(buffer, buffer, PaddingMode.None, out var bytesWritten) && bytesWritten == buffer.Length;
    }

    public static bool TryRestoreData(ReadOnlySpan<byte> data, scoped Span<byte> restored, uint magic)
    {
        for (var i = 0; i < data.Length / 16; i++)
        {
            var posData = i * 16;
            var posRestore = i * 12;

            var key = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(posData));
            if (key != magic)
            {
                return false;
            }
                    
            data.Slice(posData + 4, 12).CopyTo(restored.Slice(posRestore));
        }
        
        return true;
    }
}