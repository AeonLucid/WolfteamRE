// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-24.

using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using Serilog;
using Wolfteam.Packets;
using Wolfteam.Packets.Channel;
using Wolfteam.Packets.Channel.Data;
using Wolfteam.Packets.Datagram;
using Wolfteam.Packets.Unknown;

namespace Wolfteam.Server.Channel;

public class ChannelConnection : WolfGameConnection
{
    private static readonly ILogger Logger = Log.ForContext<ChannelConnection>();
    private static readonly ClientVersion ClientVersion = ClientVersion.IS_854;
    
    public ChannelConnection(Guid id, Socket client) : base(Logger, ClientVersion, id, client)
    {
        TcpAddress = (IPEndPoint?)client.RemoteEndPoint;
    }

    public IPEndPoint? TcpAddress { get; set; }
    public IPEndPoint? UdpAddress { get; set; }

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
                    Uk9 = "One",    // Mute releated
                    Uk10 = "Two",  // Mute releated
                    MuteBanReason = "Three",
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
                if (TcpAddress == null || UdpAddress == null)
                {
                    Logger.Warning("Missing Tcp/Udp address for {@Packet}", packet);
                    return;
                }
                
                var tcpIp = TcpAddress.Address.GetAddressBytes();
                var tcpPort = (ushort)TcpAddress.Port;
                
                var udpIp = UdpAddress.Address.GetAddressBytes();
                var udpPort = (ushort)UdpAddress.Port;

                var zzz = BinaryPrimitives.ReadUInt32LittleEndian(tcpIp);
                
                await SendPacketAsync(new CS_CK_UDPSUCCESS_ACK
                {
                    // Tcp ip/port
                    Uk1 = BinaryPrimitives.ReadUInt32LittleEndian(tcpIp),
                    Uk2 = tcpPort,
                    // Udp ip/port
                    Uk3 = BinaryPrimitives.ReadUInt32LittleEndian(udpIp),
                    Uk4 = udpPort
                });
                break;
            case CS_CK_ALIVE_REQ:
                // Do nothing.
                break;
            case CS_CH_USERINFO_REQ:
                await SendPacketAsync(new CS_CH_USERINFO_ACK
                {
                    NickName = "AeonLucid",
                    Uk2 = 1,
                    Uk3 = 2,
                    Uk4 = 3,
                    CurrencyGold = 300,
                    CurrencyWC = 310,
                    CurrencyCash = 400,
                    Uk8 = 100,
                    Uk9 = 320,
                    Uk10 = 4698,
                    Uk11 = 25,
                    Uk12 = 46,
                    Ranking = 213,
                    Uk14 = 6307,
                    Uk15 = 850,
                    Uk16 = 525,
                    Uk17 = 463,
                    Uk18 = 5636,
                    Kill = 4564,
                    Death = 3456,
                    Uk21 = 5435,
                    Uk22 = 4355,
                    Uk23 = 3423,
                    Uk24 = 1235,
                    Uk25 = 33,
                    Uk26 = 5744,
                    Uk27 = "Hello",
                    Uk28 = 643,
                    PrideName = "Morning",
                    Uk30 = "Sunshine",
                    Uk31 = 452,
                    Uk32 = 52,
                    Uk33 = "Testing33",
                    Uk34 = 333,
                    Uk35 = "Dunno",
                    Uk36 = "Testing444",
                    Uk37 = 10,
                    Uk38 = "YesNoMaybe",
                    Uk39 = 55,
                    Uk40 = 44,
                    Uk41 = "Meow",
                    Uk42 = new UserInfoUk42[]
                    {
                        new UserInfoUk42
                        {
                            Uk1 = "Unknownnnn"
                        }
                    },
                    Uk43 = new ushort[]
                    {
                        0xABCD,
                        0x1234
                    }
                });
                break;
            case CS_IN_ITEM_LIMITED_REQ:
                await SendPacketAsync(new CS_IN_ITEM_LIMITED_ACK
                {
                    Uk1_ArraySize = 0x00
                });
                break;
            case CS_CH_EVENTSHOPINFO_REQ:
                await SendPacketAsync(new CS_CH_EVENTSHOPINFO_ACK
                {
                    Uk1_ArraySize = 0,
                    Uk2_ArraySize = 0,
                    Uk3 = 0
                });
                break;
            case CS_CH_CRESTUSERINFO_REQ:
                await SendPacketAsync(new CS_CH_CRESTUSERINFO_ACK
                {
                    Unused = 0,
                    Uk1 = new RestUserInfoUk1[]
                    {
                        new RestUserInfoUk1
                        {
                            Uk1 = 0xAA,
                            Uk2 = 0xBBBB
                        }
                    },
                    Uk2 = new RestUserInfoUk2[]
                    {
                        new RestUserInfoUk2
                        {
                            Uk1 = 123,
                            Uk2 = 456,
                            Uk3 = 1
                        }
                    },
                    Uk3 = 4
                });
                break;
            case CS_IN_PACKAGE_ITEM_LIST_REQ:
                await SendPacketAsync(new CS_IN_PACKAGE_ITEM_LIST_ACK
                {
                    Uk1 = [
                        0xBEEF
                    ],
                    Uk2_ArraySize = 0x00
                });
                break;
            case CS_IN_ITEMPRICE_VERSION_REQ:
                await SendPacketAsync(new CS_IN_ITEMPRICE_VERSION_ACK
                {
                    Uk1 = 0x00,
                    Uk2 = 0x00
                });
                break;
            case CS_IN_ITEMLIST_REQ:
                await SendPacketAsync(new CS_IN_ITEMLIST_ACK
                {
                    Uk1_ArraySize = 0x00,
                    Uk2 = 0xF3
                });
                break;
            case CS_IN_EQUIPLIST_REQ:
                await SendPacketAsync(new CS_IN_EQUIPLIST_ACK
                {
                    Uk1_ArraySize = 0
                });
                break;
            case CS_IN_CHARITEMLIST_REQ:
                await SendPacketAsync(new CS_IN_CHARITEMLIST_ACK
                {
                    Uk1_Bool = 1,
                    Uk2_ArraySize = 0
                });
                break;
            case CS_IN_CHARINFO_REQ:
                await SendPacketAsync(new CS_IN_CHARINFO_ACK
                {
                    Uk1_ArraySize = 0
                });
                break;
            case CS_CH_GETEVENTGOLDINFO_REQ:
                await SendPacketAsync(new CS_CH_GETEVENTGOLDINFO_ACK
                {
                    Uk1_ArraySize = 0
                });
                break;
            case CS_CH_GETMACROMESSAGE_REQ:
                await SendPacketAsync(new CS_CH_GETMACROMESSAGE_ACK
                {
                    Uk1_ArraySize = 0
                });
                break;
            case CS_CH_GETPOWERUSERINFO_REQ:
                await SendPacketAsync(new CS_CH_GETPOWERUSERINFO_ACK
                {
                    Uk1 = 0,
                    Uk2 = 0,
                    Uk3 = 0,
                    Uk4 = "TestHiAAA"
                });
                break;
            case CS_CH_SELECTCHAR_REQ:
                await SendPacketAsync(new CS_CH_SELECTCHAR_ACK
                {
                    Uk1 = 0xFFFFFFFF,   // -1
                    Uk2 = 0xFFFF,       // -1
                    Uk3_ArraySize = 0
                });
                break;
            case CS_CH_DISCONNECT_REQ:
                await SendPacketAsync(new CS_CH_DISCONNECT_ACK());
                break;
            case CS_FD_USEHACKTOOL_REQ useHackToolReq:
                Logger.Warning("Client has unwanted files {@Files}", useHackToolReq.Uk4);
                break;
            default:
                Logger.Warning("Unhandled packet {@Packet}", packet);
                break;
        }
    }
}