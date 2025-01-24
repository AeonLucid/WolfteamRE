// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-22.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace Wolfteam.Packets;

public ref struct SpanWriter
{
    private readonly Span<byte> _data;

    public SpanWriter(Span<byte> data)
    {
        _data = data;
        Position = 0;
    }
    
    public int Position { get; private set; }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CheckBounds(int count)
    {
        if (Position + count > _data.Length)
        {
            throw new InvalidOperationException("Attempted to write past the end of the buffer.");
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Advance(int count)
    {
        Position += count;
    }
    
    public void WriteS8(sbyte value)
    {
        CheckBounds(1);
        _data[Position] = (byte)value;
        Advance(1);
    }
    
    public void WriteU8(byte value)
    {
        CheckBounds(1);
        _data[Position] = value;
        Advance(1);
    }
    
    public void WriteS16(short value)
    {
        CheckBounds(2);
        BinaryPrimitives.WriteInt16LittleEndian(_data.Slice(Position), value);
        Advance(2);
    }
    
    public void WriteU16(ushort value)
    {
        CheckBounds(2);
        BinaryPrimitives.WriteUInt16LittleEndian(_data.Slice(Position), value);
        Advance(2);
    }
    
    public void WriteU16BE(ushort value)
    {
        CheckBounds(2);
        BinaryPrimitives.WriteUInt16BigEndian(_data.Slice(Position), value);
        Advance(2);
    }
    
    public void WriteS32(int value)
    {
        CheckBounds(4);
        BinaryPrimitives.WriteInt32LittleEndian(_data.Slice(Position), value);
        Advance(4);
    }
    
    public void WriteU32(uint value)
    {
        CheckBounds(4);
        BinaryPrimitives.WriteUInt32LittleEndian(_data.Slice(Position), value);
        Advance(4);
    }

    public void WriteS64(long value)
    {
        CheckBounds(8);
        BinaryPrimitives.WriteInt64LittleEndian(_data.Slice(Position), value);
        Advance(8);
    }
    
    public void WriteFloat(float value)
    {
        CheckBounds(4);
        BinaryPrimitives.WriteSingleLittleEndian(_data.Slice(Position), value);
        Advance(4);
    }
    
    public void WriteBytes(scoped ReadOnlySpan<byte> bytes)
    {
        CheckBounds(bytes.Length);
        
        if (bytes.Length == 0) return;
        
        bytes.CopyTo(_data.Slice(Position));
        Advance(bytes.Length);
    }
    
    public void WriteString(Encoding encoding, string value)
    {
        var byteCount = encoding.GetByteCount(value);
        if (byteCount > byte.MaxValue)
        {
            throw new InvalidOperationException("String is too long to write.");
        }
        
        Span<byte> bytes = stackalloc byte[byteCount];
        
        encoding.GetBytes(value, bytes);
        
        WriteBytes(bytes);
    }
}