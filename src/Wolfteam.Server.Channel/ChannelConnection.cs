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
                    Uk1 = 0x7B,
                    Uk2 = 0x6F855,
                    Uk3 = 1,
                    Uk4 = 1,
                    ChannelType = 0, // 0 = lobby, 1 = pride battle championship lobby, 2 = pride battle lobby, 3 = ranking, 4 = lobby with ranks
                    Uk6 = 1,
                    IsMuted = 0,
                    EventTheme = 6, // 1 = chinese new year, 2 = valentine, 3 = golden egg, 4 = independence day, 8 = halloween
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
                    Uk11 = 1,
                    Class = 61,
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
            case CS_CN_CHANGESHEET_REQ changeSheetAck:
                Logger.Information("Changed sheet to {Uk1}", changeSheetAck.Uk1);
                
                await SendPacketAsync(new CS_CN_CHANGESHEET_ACK
                {
                    Uk1 = changeSheetAck.Uk1
                });
                break;
            case CS_FD_FIELDLIST_REQ fieldlistReq:
                Logger.Information("Requesting field list {@FieldList}", fieldlistReq);
            
                await SendPacketAsync(new CS_FD_FIELDLIST_ACK
                {
                    Uk1 = 3,
                    Uk2 = 5,
                    Uk3 = [
                        new FieldListEntry
                        {
                            FieldId = 0x111,
                            Title = "Game 1",
                            IsProtected = 0,
                            IsPlaying = 0,
                            Map = 60,
                            Mode = 2,
                            Uk7 = 1, // Maybe SubMode?
                            Time = 600,
                            Mission = 100,
                            Uk10 = 0,
                            Uk11 = 0,
                            Uk12 = 0,
                            PlayerMax = 16,
                            PlayerCount = 0,
                            Uk15 = 0,
                            Uk16 = 0,
                            Uk17 = 0, // 1 = green room name
                            ClassMax = 0,
                            ClassMin = 0x3D
                        },
                        new FieldListEntry
                        {
                            FieldId = 0x112,
                            Title = "Game 2",
                            IsProtected = 0,
                            IsPlaying = 0,
                            Map = 60,
                            Mode = 2,
                            Uk7 = 1, // Maybe SubMode?
                            Time = 600,
                            Mission = 100,
                            Uk10 = 1,
                            Uk11 = 1,
                            Uk12 = 1,
                            PlayerMax = 16,
                            PlayerCount = 0,
                            Uk15 = 1, // 0 = white row, 1 = gray
                            Uk16 = 0,
                            Uk17 = 0, // 0 = white/gray, 1 = green room name
                            ClassMax = 0,
                            ClassMin = 0x3D
                        }
                    ]
                });
                break;
            case CS_FD_CREATE_REQ createAck:
                Logger.Information("Creating room {@Room}", createAck);
                
                await SendPacketAsync(new CS_FD_CREATE_ACK
                {
                    Unused = 0,
                    Uk2 = 1235,
                    Uk3 = 123,
                    Uk4 = 12
                });

                await SendPacketAsync(new CS_FD_CHARS_ACK
                {
                    Uk1 = 1, // 1 Allows invite and inventory stuff, 0 disables it. (I thought)
                    OwnerCharId = 0,
                    FieldId = 64, // Visually it's + 1.
                    Uk4 = createAck.Title,
                    Uk5 = createAck.Password,
                    Uk6 = createAck.MapId,
                    Uk7 = createAck.Mode,
                    Uk8 = 0,
                    UnusedArray = 0,
                    Uk10 = createAck.Time,
                    Uk11 = createAck.Mission,
                    FlagTresspass = createAck.FlagTresspass, // Effect on "Trespass" and "Observe"
                    FlagHero = createAck.FlagHero, // Effect on "Hero" and "Switch offense and defense"
                    FlagHeavy = createAck.FlagHeavy,
                    PlayerLimit = createAck.PlayerLimit,
                    Uk16 = 0,
                    Uk17 = 0,
                    Uk18 = 0,
                    IsPowerRoom = 0,
                    Uk20 =
                    [
                        new FieldCharEntry4
                        {
                            Uk1 = 0,
                            Team = 1, // 0 = ?, 1 = Red, 2 = Blue
                            Position = 0,
                            
                            Uk4 = 0, // 0 = Play, 1 = Ready
                            ConnectionId = 0x7B,
                            Uk6 = 0,
                            Uk7 = 0,
                            Class = 61,
                            Uk9 = 0,
                            Uk10 = 0,
                            UkAA = string.Empty,
                            NickName = "AeonLucid",
                            Pride = "Morning",
                            Uk11 = string.Empty,
                            Uk11_1 = 0,
                            Uk12 = 0,
                            Uk13 = 0,
                            Uk14 = 0,
                            Uk15 = 0,
                            Uk16 = 1, // 0 = Play, 1 = Ready
                            Uk17 = 0,
                            Uk18 = [],
                            Uk19 = [],
                            Uk20 = 0,
                            Uk21 = 0,
                            Uk22 = []
                        },
                        new FieldCharEntry4
                        {
                            Uk1 = 1,
                            Team = 2, // 0 = ?, 1 = Red, 2 = Blue
                            Position = 0,
                            
                            Uk4 = 0, // 0 = Play, 1 = Ready
                            ConnectionId = 0x7C,
                            Uk6 = 0,
                            Uk7 = 0,
                            Class = 61,
                            Uk9 = 0,
                            Uk10 = 0,
                            UkAA = string.Empty,
                            NickName = "Meow",
                            Pride = "Morning",
                            Uk11 = string.Empty,
                            Uk11_1 = 0,
                            Uk12 = 0,
                            Uk13 = 0,
                            Uk14 = 0,
                            Uk15 = 0,
                            Uk16 = 1, // 0 = Play, 1 = Ready
                            Uk17 = 0,
                            Uk18 = [],
                            Uk19 = [],
                            Uk20 = 0,
                            Uk21 = 0,
                            Uk22 = []
                        }
                    ],
                    Uk21 = createAck.ClassMax,
                    Uk22 = createAck.ClassMin
                });
                break;
            case CS_FD_EXIT_REQ exitReq:
                Logger.Information("Exiting room {@Room}", exitReq);
                
                await SendPacketAsync(new CS_FD_EXIT_ACK
                {
                    Uk1 = 0
                });
                break;
            case CS_FD_INITFIELD_REQ:
                await SendPacketAsync(new CS_FD_INITFIELD_ACK());
                break;
            case CS_CH_LOGOUT_REQ:
                await SendPacketAsync(new CS_CH_LOGOUT_ACK());
                break;
            default:
                Logger.Warning("Unhandled packet {PacketId}", id);
                break;
        }
    }
}