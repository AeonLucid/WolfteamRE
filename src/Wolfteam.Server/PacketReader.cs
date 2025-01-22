// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-21.

using System.Buffers;

namespace Wolfteam.Server;

public ref struct PacketReader
{
    private ReadOnlySequence<byte> _buffer;
    private SequenceReader<byte> _reader;

    public PacketReader(ReadOnlySequence<byte> buffer)
    {
        _buffer = buffer;
        _reader = new SequenceReader<byte>(buffer);
    }
    
    public long Remaining => _reader.Remaining;
    public bool Completed => _reader.End;
    
    public byte ReadByte()
    {
        if (!_reader.TryRead(out var value))
        {
            throw new PacketReaderException("Failed to read byte");
        }
        
        return value;
    }
    
    public short ReadShort()
    {
        if (!_reader.TryReadLittleEndian(out short value))
        {
            throw new PacketReaderException("Failed to read short");
        }
        
        return value;
    }
    
    public int ReadInt()
    {
        if (!_reader.TryReadLittleEndian(out int value))
        {
            throw new PacketReaderException("Failed to read int");
        }
        
        return value;
    }
    
    public long ReadLong()
    {
        if (!_reader.TryReadLittleEndian(out long value))
        {
            throw new PacketReaderException("Failed to read long");
        }
        
        return value;
    }

    public ReadOnlySequence<byte> ReadBytes(int len)
    {
        if (!_reader.TryReadExact(len, out var sequence))
        {
            throw new PacketReaderException("Failed to read bytes");
        }
        
        return sequence;
    }
}