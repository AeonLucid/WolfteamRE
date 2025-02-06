// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-21.

using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using Serilog;
using Wolfteam.Server.Crypto;
using static Wolfteam.Server.Login.Constants;

namespace Wolfteam.Server.Login.Packets;

public static class LoginRequest
{
    private static readonly ILogger Logger = Log.ForContext(typeof(LoginRequest));
    
    public static bool TryParseStatic(Aes crypto, ref ReadOnlySequence<byte> data, [NotNullWhen(true)] out string? username, out uint magic)
    {
        // Read and decrypt into buffer.
        Span<byte> buffer = stackalloc byte[32];

        if (!WolfCrypto.TryReadEncrypted(crypto, buffer, ref data))
        {
            username = null;
            magic = 0;
            return false;
        }
        
        // Read username.
        var usernameLen = buffer.IndexOf((byte) 0);
        if (usernameLen == -1)
        {
            usernameLen = 16;
        }
        
        username = Encoding.UTF8.GetString(buffer.Slice(0, usernameLen));
        magic = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(16, sizeof(uint)));
        return true;
    }
    
    public static bool TryDecryptAuth(Aes crypto, ref ReadOnlySequence<byte> data, out string? password, out int version)
    {
        // Read and decrypt into buffer.
        Span<byte> buffer = stackalloc byte[32];
        
        if (!WolfCrypto.TryReadEncrypted(crypto, buffer, ref data))
        {
            Logger.Warning("Failed to decrypt auth packet");
            
            password = null;
            version = 0;
            return false;
        }
        
        // Restore data without magic.
        var restoreLength = (buffer.Length / 16) * 12;
        var restored = restoreLength > 256 ? new byte[restoreLength] : stackalloc byte[restoreLength];

        if (!WolfCrypto.TryRestoreData(buffer, restored, PacketMagic))
        {
            Logger.Warning("Failed to restore auth packet, invalid magic");
            
            password = null;
            version = 0;
            return false;
        }
        
        // Read variables.
        password = Encoding.UTF8.GetString(restored.Slice(0, 20));
        version = BinaryPrimitives.ReadInt32LittleEndian(restored.Slice(20, sizeof(int)));
        return true;
    }
}