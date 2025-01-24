// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-22.

using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace Wolfteam.Packets;

// Read data from a Span<byte>, advancing the position as it goes.
// Use BinaryPrimitives to read values from the Span.
public ref struct SpanReader
{
    private ReadOnlySpan<byte> _data;
    
    public SpanReader(ReadOnlySpan<byte> data)
    {
        _data = data;
    }
    
    public int Remaining => _data.Length;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Advance(int count)
    {
        _data = _data.Slice(count);
    }
    
    public bool TryReadU8(out byte value)
    {
        if (_data.Length < 1)
        {
            value = 0;
            return false;
        }

        value = _data[0];
        Advance(1);
        return true;
    }

    public bool TryReadU16(out ushort value)
    {
        if (_data.Length < 2)
        {
            value = 0;
            return false;
        }

        value = BinaryPrimitives.ReadUInt16LittleEndian(_data);
        Advance(2);
        return true;
    }

    public bool TryReadU16BE(out ushort value)
    {
        if (_data.Length < 2)
        {
            value = 0;
            return false;
        }

        value = BinaryPrimitives.ReadUInt16BigEndian(_data);
        Advance(2);
        return true;
    }
    
    public bool TryReadU32(out uint value)
    {
        if (_data.Length < 4)
        {
            value = 0;
            return false;
        }

        value = BinaryPrimitives.ReadUInt32LittleEndian(_data);
        Advance(4);
        return true;
    }

    public bool TryReadString(Encoding encoding, int length, [NotNullWhen(true)] out string? value)
    {
        if (_data.Length < length)
        {
            value = null;
            return false;
        }

        value = encoding.GetString(_data.Slice(0, length));
        Advance(length);
        return true;
    }

    public bool TryReadBytes(int length, [NotNullWhen(true)] out byte[]? value)
    {
        if (_data.Length < length)
        {
            value = null;
            return false;
        }

        value = _data.Slice(0, length).ToArray();
        Advance(length);
        return true;
    }
}