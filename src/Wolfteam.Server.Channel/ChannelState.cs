// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-28.

using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Wolfteam.Packets.Channel;
using Wolfteam.Server.Channel.State;

namespace Wolfteam.Server.Channel;

/// <summary>
///     Holds the state of the channel server. Fields, players etc.
/// </summary>
public class ChannelState
{
    private readonly ConcurrentDictionary<ushort, PlayerSession> _players;
    private readonly ConcurrentDictionary<ushort, Field> _fields;
    
    private ushort _nextSessionId;
    private object _addFieldLock;

    public ChannelState()
    {
        _players = new ConcurrentDictionary<ushort, PlayerSession>();
        _fields = new ConcurrentDictionary<ushort, Field>();
        _nextSessionId = 1;
        _addFieldLock = new object();
    }

    public PlayerSession AddPlayer(ChannelConnection connection, string username)
    {
        var sessionId = Interlocked.Exchange(ref _nextSessionId, (ushort)(_nextSessionId + 1));
        var sessionKey = GetRandomSessionKey();
        var sessionPlayer = new PlayerSession(sessionId, sessionKey, connection, username);

        if (!_players.TryAdd(sessionId, sessionPlayer))
        {
            throw new InvalidOperationException($"Failed to add player to players dictionary, id {sessionId} already exists");
        }

        return sessionPlayer;
    }

    public bool RemovePlayer(ushort sessionId)
    {
        return _players.TryRemove(sessionId, out _);
    }

    public bool TryGetPlayer(ushort sessionId, [NotNullWhen(true)] out PlayerSession? player)
    {
        return _players.TryGetValue(sessionId, out player);
    }

    public Field CreateField(CS_FD_CREATE_REQ request)
    {
        lock (_addFieldLock)
        {
            var fieldId = GetNextFieldId();
            var field = new Field(fieldId, request);
            
            if (!_fields.TryAdd(fieldId, field))
            {
                throw new InvalidOperationException($"Failed to add field to fields dictionary, id {fieldId} already exists");
            }
            
            return field;
        }
    }

    public Field? GetField(ushort fieldId)
    {
        lock (_addFieldLock)
        {
            return _fields.GetValueOrDefault(fieldId);
        }
    }

    /// <summary>
    ///     Get all current fields.
    /// </summary>
    public Field[] GetFields()
    {
        lock (_addFieldLock)
        {
            return _fields.Values.ToArray();
        }
    }
    
    private ushort GetNextFieldId()
    {
        for (var i = 0; i < ushort.MaxValue; i++)
        {
            if (!_fields.ContainsKey((ushort)i))
            {
                return (ushort)i;
            }   
        }
        
        throw new InvalidOperationException("Failed to find a free field id");
    }
    
    private static uint GetRandomSessionKey()
    {
        Span<byte> buffer = stackalloc byte[4];
        
        RandomNumberGenerator.Fill(buffer);

        return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
    }
}