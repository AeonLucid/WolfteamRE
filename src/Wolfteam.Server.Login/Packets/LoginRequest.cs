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
    
    public static bool TryParseStatic(Aes crypto, ref PacketReader reader, [NotNullWhen(true)] out string? username, out uint magic)
    {
        // Read and decrypt into buffer.
        Span<byte> buffer = stackalloc byte[32];

        if (!TryReadEncrypted(crypto, buffer, ref reader))
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
    
    public static bool TryDecryptAuth(Aes crypto, ref PacketReader reader, out string? password, out int version)
    {
        // Read and decrypt into buffer.
        Span<byte> buffer = stackalloc byte[32];
        
        if (!TryReadEncrypted(crypto, buffer, ref reader))
        {
            Logger.Warning("Failed to decrypt auth packet");
            
            password = null;
            version = 0;
            return false;
        }
        
        // Restore data without magic.
        Span<byte> restored = stackalloc byte[(buffer.Length / 16) * 12];

        if (!TryRestoreData(buffer, restored, PacketMagic))
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

    public static byte[] CreateAuthKey(string username, ReadOnlySpan<byte> passwordHash, uint magic)
    {
        if (passwordHash.Length != 16)
        {
            throw new ArgumentException("Password hash must be 16 bytes", nameof(passwordHash));
        }
        
        // Convert username to span.
        var usernameLen = Encoding.UTF8.GetByteCount(username);
        Span<byte> usernamePart = stackalloc byte[usernameLen];
        
        Encoding.UTF8.GetBytes(username, usernamePart);
        
        // Convert password to span.
        Span<char> passwordHexChars = stackalloc char[20];
        Span<byte> passwordPart = stackalloc byte[20];

        if (!Convert.TryToHexStringLower(passwordHash.Slice(6, 10), passwordHexChars, out var written) || written != 20)
        {
            throw new InvalidOperationException("Failed to convert password hash to hex string");
        }
        
        Encoding.UTF8.GetBytes(passwordHexChars, passwordPart);
        
        // Setup hash input.
        Span<byte> hashInput = stackalloc byte[usernamePart.Length + passwordPart.Length + sizeof(uint)];
        
        usernamePart.CopyTo(hashInput);
        passwordPart.CopyTo(hashInput.Slice(usernamePart.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(hashInput.Slice(usernamePart.Length + passwordPart.Length), magic);
        
        // Hash input.        
        Span<byte> hashOutput = stackalloc byte[20];
        WolfSHA1.Hash(hashInput, hashOutput);
                
        // Setup aes key.
        return hashOutput.Slice(0, 16).ToArray();
    }

    private static bool TryReadEncrypted(Aes crypto, scoped Span<byte> buffer, ref PacketReader reader)
    {
        reader.ReadBytes(buffer.Length).CopyTo(buffer);
        return crypto.TryDecryptEcb(buffer, buffer, PaddingMode.None, out var bytesWritten) && bytesWritten == buffer.Length;
    }

    private static bool TryRestoreData(ReadOnlySpan<byte> data, scoped Span<byte> restored, uint magic)
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