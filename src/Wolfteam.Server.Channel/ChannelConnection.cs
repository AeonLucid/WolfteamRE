// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-24.

using System.Net.Sockets;
using Serilog;
using Wolfteam.Packets;
using Wolfteam.Packets.Channel;

namespace Wolfteam.Server.Channel;

public class ChannelConnection : WolfGameConnection
{
    private static readonly ILogger Logger = Log.ForContext<ChannelConnection>();
    private static readonly ClientVersion ClientVersion = ClientVersion.IS_854;
    
    public ChannelConnection(Guid id, Socket client) : base(Logger, ClientVersion, id, client)
    {
    }

    protected override async ValueTask HandlePacketAsync(PacketId id, PacketHeader header, IWolfPacket packet)
    {
        if (packet is CS_CH_LOGIN_REQ loginReq)
        {
            await SendPacketAsync(header.Sequence, new CS_CH_LOGIN_ACK
            {
                Uk1 = 1,
                Uk2 = 2,
                Uk3 = 3,
                Uk4 = 4,
                Uk5 = 5,
                Uk6 = 6,
                Uk7 = 1,
                Uk8 = 8,
                Uk9 = "One",
                Uk10 = "Two",
                Uk11 = "Three",
                Uk12 = 1,
                Uk13 = 1,
                Uk14 = 1,
                Uk15 = 1
            });
        } 
        else if (packet is CS_CH_SNSACCOUNT_REQ snsAccountReq)
        {
            await SendPacketAsync(header.Sequence, new CS_CH_SNSACCOUNT_ACK
            {
                Uk1 = snsAccountReq.Uk1,
                Uk2 = "AlphaWTWT",
                Uk3 = "BetaWTWT",
            });
        }
    }
}