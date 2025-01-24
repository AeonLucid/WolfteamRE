// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-24.

using System.Net.Sockets;
using Serilog;
using Wolfteam.Packets;

namespace Wolfteam.Server.Channel;

public class ChannelConnection : WolfGameConnection
{
    private static readonly ILogger Logger = Log.ForContext<ChannelConnection>();
    private static readonly ClientVersion ClientVersion = ClientVersion.IS_854;
    
    public ChannelConnection(Guid id, Socket client) : base(Logger, ClientVersion, id, client)
    {
    }

    protected override ValueTask HandlePacketAsync(PacketId id, PacketHeader header, IWolfPacket packet)
    {
        return ValueTask.CompletedTask;
    }
}