using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace Wolfteam.Server.Login;

internal ref struct SpanReader
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
    
    public sbyte ReadS8()
    {
        var value = (sbyte)_data[0];
        Advance(1);
        return value;
    }
    
    public byte ReadU8()
    {
        var value = _data[0];
        Advance(1);
        return value;
    }
    
    public short ReadS16()
    {
        var value = BinaryPrimitives.ReadInt16LittleEndian(_data);
        Advance(2);
        return value;
    }
    
    public ushort ReadU16()
    {
        var value = BinaryPrimitives.ReadUInt16LittleEndian(_data);
        Advance(2);
        return value;
    }
    
    public int ReadS32()
    {
        var value = BinaryPrimitives.ReadInt32LittleEndian(_data);
        Advance(4);
        return value;
    }
    
    public uint PeekU32()
    {
        return BinaryPrimitives.ReadUInt32LittleEndian(_data);
    }
    
    public uint ReadU32()
    {
        var value = BinaryPrimitives.ReadUInt32LittleEndian(_data);
        Advance(4);
        return value;
    }
    
    public long ReadS64()
    {
        var value = BinaryPrimitives.ReadInt64LittleEndian(_data);
        Advance(8);
        return value;
    }
    
    public ulong ReadU64()
    {
        var value = BinaryPrimitives.ReadUInt64LittleEndian(_data);
        Advance(8);
        return value;
    }
    
    public float ReadFloat()
    {
        var value = BinaryPrimitives.ReadSingleLittleEndian(_data);
        Advance(4);
        return value;
    }
    
    public ReadOnlySpan<byte> ReadBytes(int count)
    {
        if (count == 0)
        {
            return ReadOnlySpan<byte>.Empty;
        }
        
        var value = _data.Slice(0, count);
        Advance(count);
        return value;
    }
}