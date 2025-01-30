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
using Wolfteam.Server.Channel.State;

namespace Wolfteam.Server.Channel;

public class ChannelConnection : WolfGameConnection
{
    private static readonly ILogger Logger = Log.ForContext<ChannelConnection>();
    
    private readonly ChannelState _state;
    
    public ChannelConnection(Guid id, Socket client, ClientVersion clientVersion, ChannelState state) : base(Logger, clientVersion, id, client)
    {
        _state = state;
    }

    public PlayerSession? Player { get; private set; }

    public override async ValueTask HandlePacketAsync(PacketId id, IWolfPacket packet)
    {
        switch (packet)
        {
            case CS_CH_LOGIN_REQ loginReq:
                if (string.IsNullOrEmpty(loginReq.Username))
                {
                    Close("Login username is empty");
                    return;
                }
                
                Player = _state.AddPlayer(this, loginReq.Username);
                
                await SendPacketAsync(new CS_CH_LOGIN_ACK
                {
                    SessionId = Player.SessionId,
                    SessionKey = Player.SessionKey,
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
            case CS_UD_UDPADDR_REQ udpAddrReq:
                if (Player == null)
                {
                    Close("Player is null");
                    return;
                }
                
                Player.UdpConnectionDetails.LocalEndPoint = new IPEndPoint(udpAddrReq.IpAddress, udpAddrReq.Port);
                
                await SendPacketAsync(new CS_CH_UDPADDR_ACK());
                break;
            case CS_CK_UDPSUCCESS_REQ:
            {
                if (Player == null)
                {
                    Close("Player is null");
                    return;
                }
                
                if (Player.UdpConnectionDetails.RemoteEndPoint == null)
                {
                    Close("UdpConnectionDetails.RemoteEndPoint is null");
                    return;
                }
                
                if (Player.UdpConnectionDetails.LocalEndPoint == null)
                {
                    Close("UdpConnectionDetails.LocalEndPoint is null");
                    return;
                }
                
                var remoteIp = Player.UdpConnectionDetails.RemoteEndPoint.Address.GetAddressBytes();
                var remotePort = (ushort)Player.UdpConnectionDetails.RemoteEndPoint.Port;
                
                var localIp = Player.UdpConnectionDetails.LocalEndPoint.Address.GetAddressBytes();
                var localPort = (ushort)Player.UdpConnectionDetails.LocalEndPoint.Port;
                
                // Debug IPs.
                Logger.Debug("Remote UDP {IpEndPoint}", Player.UdpConnectionDetails.RemoteEndPoint);
                Logger.Debug("Local UDP {IpEndPoint}", Player.UdpConnectionDetails.LocalEndPoint);
                
                await SendPacketAsync(new CS_CK_UDPSUCCESS_ACK
                {
                    RemoteIp = BinaryPrimitives.ReadUInt32LittleEndian(remoteIp),
                    RemotePort = remotePort,
                    LocalIp = BinaryPrimitives.ReadUInt32LittleEndian(localIp),
                    LocalPort = localPort
                });
                break;
            }
            case CS_CK_ALIVE_REQ:
                // Do nothing.
                break;
            case CS_CH_USERINFO_REQ:
                if (Player == null)
                {
                    Close("Player is null");
                    return;
                }
                
                await SendPacketAsync(new CS_CH_USERINFO_ACK
                {
                    NickName = Player.Username,
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

                var fields = _state.GetFields();
                var fieldEntries = fields.Select(x => new FieldListEntry
                {
                    FieldId = x.Id,
                    Title = x.Title,
                    IsProtected = (byte)(string.IsNullOrEmpty(x.Password) ? 0 : 1),
                    IsPlaying = 0,
                    Map = x.MapId,
                    Mode = x.Mode,
                    Wins = x.Wins,
                    Time = x.Time,
                    Mission = x.Mission,
                    Uk10 = x.FlagTresspass, // TODO: Check tresspass
                    Uk11 = x.FlagHero,
                    Uk12 = x.FlagHeavy,
                    PlayerMax = x.PlayerLimit,
                    PlayerCount = x.PlayerCount,
                    Uk15 = 0,
                    Uk16 = 0,
                    IsPowerRoom = x.IsPowerRoom,
                    ClassMax = x.ClassMax,
                    ClassMin = x.ClassMin
                }).ToArray();
                
                await SendPacketAsync(new CS_FD_FIELDLIST_ACK
                {
                    Uk1 = 3,
                    Uk2 = 5,
                    Uk3 = fieldEntries
                });
                break;
            case CS_FD_CREATE_REQ createAck:
            {
                if (Player == null)
                {
                    Close("Player is null");
                    return;
                }
                
                Logger.Information("Creating room {@Room}", createAck);

                // Create field.
                var field = _state.CreateField(createAck);
                
                // Add player to field.
                if (!await field.AddPlayer(Player, FieldAddReason.Create))
                {
                    Close("Failed to add player to newly created field");
                    return;
                }
                break;
            }
            case CS_FD_ENTER_REQ enterReq:
            {
                if (Player == null)
                {
                    Close("Player is null when entering room");
                    return;
                }
                
                if (Player.FieldChar != null)
                {
                    Close("Player.Field is not null when entering room");
                    return;
                }
                
                // Add player to field.
                Logger.Information("Entering room {@Room}", enterReq);

                // Get field.
                var field = _state.GetField(enterReq.FieldId);
                if (field == null)
                {
                    await SendPacketAsync(new CS_FD_ENTER_ACK(), ErrorCode.S126_RoomDoesNotExist);
                    return;
                }
                
                // Check password.
                var fieldPassword = field.Password;
                if (!string.IsNullOrEmpty(fieldPassword) && !fieldPassword.Equals(enterReq.Password))
                {
                    await SendPacketAsync(new CS_FD_ENTER_ACK(), ErrorCode.S122_InvalidRoomPassword);
                    return;
                }
                
                if (!await field.AddPlayer(Player, FieldAddReason.Enter))
                {
                    await SendPacketAsync(new CS_FD_ENTER_ACK(), ErrorCode.S125_RoomSlotsFull);
                    return;
                }
                break;
            }
            case CS_FD_EXIT_REQ exitReq:
            {
                if (Player == null)
                {
                    Close("Player is null when exiting room");
                    return;
                }
                
                if (Player.FieldChar == null)
                {
                    Close("Player.Field is null when exiting room");
                    return;
                }
                
                Logger.Information("Exiting room {@Room}", exitReq);
                
                var fieldChar = Player.FieldChar;
                var field = fieldChar.Field;

                await field.RemovePlayer(fieldChar.Slot);
                break;
            }
            case CS_FD_INITFIELD_REQ:
                await SendPacketAsync(new CS_FD_INITFIELD_ACK());
                break;
            case CS_FD_UDPSTART_REQ:
            {
                if (Player == null)
                {
                    Close("Player is null");
                    return;
                }
                
                var fieldChar = Player.FieldChar;
                if (fieldChar == null)
                {
                    Close("Player.FieldChar is null");
                    return;
                }
                
                await fieldChar.Field.StartUdpAsync(fieldChar);
                break;
            }
            case CS_FD_UDPREADY_REQ udpReadyReq:
            {
                if (Player == null)
                {
                    Close("Player is null");
                    return;
                }
                
                var fieldChar = Player.FieldChar;
                if (fieldChar == null)
                {
                    Close("Player.FieldChar is null");
                    return;
                }
                
                await fieldChar.Field.ReadyUdpAsync(fieldChar, udpReadyReq.Ready);
                break;
            }
            case CS_FD_STARTGAME_REQ startGameReq:
            {
                if (Player == null)
                {
                    Close("Player is null");
                    return;
                }
                
                var fieldChar = Player.FieldChar;
                if (fieldChar == null)
                {
                    Close("Player.FieldChar is null");
                    return;
                }
                
                await fieldChar.Field.StartGameAsync(fieldChar);
                break;
            }
            case CS_FD_CHANGETEAM_REQ changeTeamReq:
            {
                if (Player == null)
                {
                    Close("Player is null");
                    return;
                }
                
                var fieldChar = Player.FieldChar;
                if (fieldChar == null)
                {
                    Close("Player.FieldChar is null");
                    return;
                }

                if (!await fieldChar.Field.ChangeTeam(fieldChar.Slot))
                {
                    await SendPacketAsync(new CS_FD_CHANGETEAM_ACK(), ErrorCode.S149_InvalidSlotNumber);
                }
                break;
            }
            case CS_FD_CRM_POPUP_REQ:
            {
                await SendPacketAsync(new CS_FD_CRM_POPUP_ACK
                {
                    Uk1 = 0x7D,
                    Uk2_ArraySize = 0
                });
                break;
            }
            case CS_IN_BUYNOW_ITEMLIST_REQ:
                await SendPacketAsync(new CS_IN_BUYNOW_ITEMLIST_ACK
                {
                    Uk1 = 0,
                    Uk2 = 0
                });
                break;
            case CS_CN_ADVERTALL_REQ:
                // TODO: Send CS_CN_CHARS_ACK
                await SendPacketAsync(new CS_CN_ADVERTALL_ACK
                {
                    Uk1 = 0,
                    Uk2 = [],
                    Uk3 = 0,
                    Uk4 = [],
                    Uk5 = 0,
                    Uk6 = [],
                    Uk7 = 0,
                    Uk8 = []
                });
                break;
            case CS_PR_BATTLERESULT_REQ:
            {
                await SendPacketAsync(new CS_PR_BATTLERESULT_ACK());
                break;
            }
            case CS_GI_ITEM_COUNT_REQ:
            {
                // TODO: Shows a popup on login, is this normal?
                // await SendPacketAsync(new CS_GI_ITEM_COUNT_ACK
                // {
                //     Uk1 = 1300,
                //     Uk2 = 14
                // });
                break;
            }
            case CS_GI_PERIOD_END_REQ:
            {
                // await SendPacketAsync(new CS_GI_PERIOD_END_ACK
                // {
                //     Uk1_ArraySize = 0
                // });
                break;
            }
            case CS_CH_LOGOUT_REQ:
                await SendPacketAsync(new CS_CH_LOGOUT_ACK());
                break;
            default:
                Logger.Warning("Unhandled packet {PacketId}", id);
                break;
        }
    }
}