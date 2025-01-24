// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-24.

using System.Net.Sockets;
using Serilog;
using Wolfteam.Packets;
using Wolfteam.Packets.Channel;
using Wolfteam.Packets.Datagram;
using Wolfteam.Packets.Unknown;

namespace Wolfteam.Server.Channel;

public class ChannelConnection : WolfGameConnection
{
    private static readonly ILogger Logger = Log.ForContext<ChannelConnection>();
    private static readonly ClientVersion ClientVersion = ClientVersion.IS_854;
    
    public ChannelConnection(Guid id, Socket client) : base(Logger, ClientVersion, id, client)
    {
    }

    public override async ValueTask HandlePacketAsync(PacketId id, IWolfPacket packet)
    {
        switch (packet)
        {
            case CS_CH_LOGIN_REQ loginReq:
                await SendPacketAsync(new CS_CH_LOGIN_ACK
                {
                    Uk1 = 123,
                    Uk2 = 456789,
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
                break;
            case CS_CH_SNSACCOUNT_REQ snsAccountReq:
                await SendPacketAsync(new CS_CH_SNSACCOUNT_ACK
                {
                    Uk1 = snsAccountReq.Uk1,
                    Uk2 = "AlphaWTWT",
                    Uk3 = "BetaWTWT",
                });
                break;
            case CS_UD_UDPADDR_REQ:
                await SendPacketAsync(new CS_CH_UDPADDR_ACK());
                break;
            case CS_CK_UDPSUCCESS_REQ:
                await SendPacketAsync(new CS_CK_UDPSUCCESS_ACK
                {
                    // Tcp ip/port
                    Uk1 = 0x0100007F,
                    Uk2 = 0xDEAD,
                    // Udp ip/port
                    Uk3 = 0x0100007F,
                    Uk4 = 0xBEEF
                });
                break;
            case CS_CK_ALIVE_REQ:
                // Do nothing.
                break;
            default:
                Logger.Warning("Unhandled packet {@Packet}", packet);
                break;
        }
    }
}