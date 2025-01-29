// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-29.

namespace Wolfteam.Server.Channel.State;

public class PlayerSession
{
    public PlayerSession(ushort sessionId, uint sessionKey, ChannelConnection connection, string username)
    {
        SessionId = sessionId;
        SessionKey = sessionKey;
        Connection = connection;
        UdpConnectionDetails = new UdpConnectionDetails();
        Username = username;
    }
    
    public ushort SessionId { get; }
    public uint SessionKey { get; }
    public ChannelConnection Connection { get; }
    public UdpConnectionDetails UdpConnectionDetails { get; }
    
    public string Username { get; }
    
    /// <summary>
    ///     <see cref="FieldChar"/> the player is currently in.
    /// </summary>
    public FieldChar? FieldChar { get; set; }
}