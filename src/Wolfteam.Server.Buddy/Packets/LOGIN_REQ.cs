// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-21.

using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Serilog;
using Wolfteam.Server.Crypto;

namespace Wolfteam.Server.Buddy.Packets;

public static class LOGIN_REQ
{
    private static readonly ILogger Logger = Log.ForContext(typeof(LOGIN_REQ));
    
    public static bool TryParseStatic(Aes crypto, ref ReadOnlySequence<byte> data, [NotNullWhen(true)] out string? username, out uint magic)
    {
        // Read and decrypt into buffer.
        var bufferLen = (int) data.Length;
        var buffer = bufferLen > 256 ? new byte[bufferLen] : stackalloc byte[bufferLen];

        if (!WolfCrypto.TryReadEncrypted(crypto, buffer, ref data))
        {
            username = null;
            magic = 0;
            return false;
        }
        
        // Read username.
        var usernameLen = buffer.IndexOf((byte) 0);
        if (usernameLen == -1 || usernameLen > 16)
        {
            usernameLen = 16;
        }
        
        username = Encoding.UTF8.GetString(buffer.Slice(0, usernameLen));
        magic = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(52, sizeof(uint)));
        return true;
    }
    
    public static bool TryDecryptAuth(Aes crypto, ref ReadOnlySequence<byte> data, out LoginOut loginData)
    {
        // Read and decrypt into buffer.
        var bufferLen = (int) data.Length;
        var buffer = bufferLen > 256 ? new byte[bufferLen] : stackalloc byte[bufferLen];
        
        if (!WolfCrypto.TryReadEncrypted(crypto, buffer, ref data))
        {
            Logger.Warning("Failed to decrypt auth packet");
            
            loginData = default;
            return false;
        }
        
        // Restore data without magic.
        var restoreLength = (buffer.Length / 16) * 12;
        var restored = restoreLength > 256 ? new byte[restoreLength] : stackalloc byte[restoreLength];
    
        if (!WolfCrypto.TryRestoreData(buffer, restored, Constants.PacketMagic))
        {
            Logger.Warning("Failed to restore auth packet, invalid magic");

            loginData = default;
            return false;
        }
        
        Logger.Debug("Restored data {Restored}", Convert.ToHexStringLower(restored));
        
        // Read variables.
        loginData = new LoginOut
        {
            Magic = BinaryPrimitives.ReadUInt32LittleEndian(restored.Slice(0)),
            Unknown1 = restored.Slice(4, 20).ToArray(),
            IpAddress = new IPAddress(BinaryPrimitives.ReadUInt32LittleEndian(restored.Slice(24))),
            Port = BinaryPrimitives.ReadUInt16BigEndian(restored.Slice(28)),
            NickName = Encoding.UTF8.GetString(restored.Slice(30, 40)).TrimEnd('\0'),
            Pride = Encoding.UTF8.GetString(restored.Slice(70, 40)).TrimEnd('\0'),
            Class = BinaryPrimitives.ReadUInt32LittleEndian(restored.Slice(110)),
            Unknown3 = BinaryPrimitives.ReadUInt32LittleEndian(restored.Slice(114)),
            Unknown4 = BinaryPrimitives.ReadUInt32LittleEndian(restored.Slice(118)),
            Unknown5 = BinaryPrimitives.ReadUInt32LittleEndian(restored.Slice(122)),
        };
        return true;
    }
}

public readonly ref struct LoginOut
{
    public uint Magic { get; init; }
    public byte[] Unknown1 { get; init; }
    public IPAddress IpAddress { get; init; }
    public ushort Port { get; init; }
    public string NickName { get; init; }
    public string Pride { get; init; }
    public uint Class { get; init; }
    public uint Unknown3 { get; init; }
    public uint Unknown4 { get; init; }
    public uint Unknown5 { get; init; }
}