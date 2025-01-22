// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-22.

using System.Buffers.Binary;

namespace Wolfteam.Server;

public readonly struct PacketHeader
{
    public const int Size = 8;
    
    public byte Random { get; init; }
    public short Id { get; init; }
    public short Sequence { get; init; }
    public short Blocks { get; init; }
    public byte Checksum { get; init; }
    
    public void Serialize(Span<byte> header)
    {
        if (header.Length < Size)
        {
            throw new InvalidOperationException("Invalid header length, must be at least 8.");
        }
        
        header[0] = Random;
        BinaryPrimitives.WriteInt16LittleEndian(header.Slice(1), Id);
        BinaryPrimitives.WriteInt16LittleEndian(header.Slice(3), Sequence);
        BinaryPrimitives.WriteInt16LittleEndian(header.Slice(5), Blocks);
        header[7] = PacketCrypto.CalculateChecksum(header.Slice(0, 8));
    }
    
    public static PacketHeader Deserialize(ReadOnlySpan<byte> header)
    {
        if (header.Length < Size)
        {
            throw new InvalidOperationException("Invalid header length, must be at least 8.");
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