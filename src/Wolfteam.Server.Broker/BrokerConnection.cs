// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-21.

using System.Net.Sockets;
using Serilog;
using Wolfteam.Packets;
using Wolfteam.Packets.Broker;
using Wolfteam.Packets.Broker.Data;

namespace Wolfteam.Server.Broker;

public class BrokerConnection : WolfGameConnection
{
    private static readonly ILogger Logger = Log.ForContext<BrokerConnection>();
    private static readonly ClientVersion ClientVersion = ClientVersion.IS_854;
    
    public BrokerConnection(Guid id, Socket client) : base(Logger, ClientVersion, id, client)
    {
    }

    public override async ValueTask HandlePacketAsync(PacketId id, IWolfPacket packet)
    {
        switch (packet)
        {
            case CS_BR_CHAINLIST_REQ:
                await SendPacketAsync(new CS_BR_CHAINLIST_ACK
                {
                    // Must be 14 entries.
                    Chainlist = Enumerable.Repeat((byte)14, 14).ToArray()
                });
                break;
            case CS_BR_WORLDLIST_REQ:
                await SendPacketAsync(new CS_BR_WORLDLIST_ACK
                {
                    // 20 entries of the same server.
                    Entries =
                    [
                        new WorldListEntry
                        {
                            IpAddress = 0x0100007F,
                            Port = 40850,
                            Uk1 = 3,
                            Uk2 = 0,
                            Uk3 = 500,
                            PlayerCount1 = 0,
                            PlayerCount2 = 500
                        },
                        new WorldListEntry
                        {
                            IpAddress = 0x0100007F,
                            Port = 40850,
                            Uk1 = 3,
                            Uk2 = 0,
                            Uk3 = 500,
                            PlayerCount1 = 0,
                            PlayerCount2 = 500
                        },
                        new WorldListEntry
                        {
                            IpAddress = 0x0100007F,
                            Port = 40850,
                            Uk1 = 3,
                            Uk2 = 0,
                            Uk3 = 500,
                            PlayerCount1 = 0,
                            PlayerCount2 = 500
                        },
                        new WorldListEntry
                        {
                            IpAddress = 0x0100007F,
                            Port = 40850,
                            Uk1 = 3,
                            Uk2 = 0,
                            Uk3 = 500,
                            PlayerCount1 = 0,
                            PlayerCount2 = 500
                        },
                        new WorldListEntry
                        {
                            IpAddress = 0x0100007F,
                            Port = 40850,
                            Uk1 = 3,
                            Uk2 = 0,
                            Uk3 = 500,
                            PlayerCount1 = 0,
                            PlayerCount2 = 500
                        },
                        new WorldListEntry
                        {
                            IpAddress = 0x0100007F,
                            Port = 40850,
                            Uk1 = 3,
                            Uk2 = 0,
                            Uk3 = 500,
                            PlayerCount1 = 0,
                            PlayerCount2 = 500
                        },
                        new WorldListEntry
                        {
                            IpAddress = 0x0100007F,
                            Port = 40850,
                            Uk1 = 3,
                            Uk2 = 0,
                            Uk3 = 500,
                            PlayerCount1 = 0,
                            PlayerCount2 = 500
                        },
                        new WorldListEntry
                        {
                            IpAddress = 0x0100007F,
                            Port = 40850,
                            Uk1 = 3,
                            Uk2 = 0,
                            Uk3 = 500,
                            PlayerCount1 = 200,
                            PlayerCount2 = 500
                        },
                        new WorldListEntry
                        {
                            IpAddress = 0x0100007F,
                            Port = 40850,
                            Uk1 = 3,
                            Uk2 = 0,
                            Uk3 = 500,
                            PlayerCount1 = 0,
                            PlayerCount2 = 500
                        },
                        new WorldListEntry
                        {
                            IpAddress = 0x0100007F,
                            Port = 40850,
                            Uk1 = 3,
                            Uk2 = 0,
                            Uk3 = 500,
                            PlayerCount1 = 0,
                            PlayerCount2 = 500
                        },
                        new WorldListEntry
                        {
                            IpAddress = 0x0100007F,
                            Port = 40850,
                            Uk1 = 3,
                            Uk2 = 0,
                            Uk3 = 500,
                            PlayerCount1 = 0,
                            PlayerCount2 = 500
                        },
                        new WorldListEntry
                        {
                            IpAddress = 0x0100007F,
                            Port = 40850,
                            Uk1 = 3,
                            Uk2 = 0,
                            Uk3 = 500,
                            PlayerCount1 = 0,
                            PlayerCount2 = 500
                        },
                        new WorldListEntry
                        {
                            IpAddress = 0x0100007F,
                            Port = 40850,
                            Uk1 = 3,
                            Uk2 = 0,
                            Uk3 = 500,
                            PlayerCount1 = 0,
                            PlayerCount2 = 500
                        },
                        new WorldListEntry
                        {
                            IpAddress = 0x0100007F,
                            Port = 40850,
                            Uk1 = 3,
                            Uk2 = 0,
                            Uk3 = 500,
                            PlayerCount1 = 0,
                            PlayerCount2 = 500
                        },
                        new WorldListEntry
                        {
                            IpAddress = 0x0100007F,
                            Port = 40850,
                            Uk1 = 3,
                            Uk2 = 0,
                            Uk3 = 500,
                            PlayerCount1 = 0,
                            PlayerCount2 = 500
                        },
                        new WorldListEntry
                        {
                            IpAddress = 0x0100007F,
                            Port = 40850,
                            Uk1 = 3,
                            Uk2 = 0,
                            Uk3 = 500,
                            PlayerCount1 = 0,
                            PlayerCount2 = 500
                        },
                        new WorldListEntry
                        {
                            IpAddress = 0x0100007F,
                            Port = 40850,
                            Uk1 = 3,
                            Uk2 = 0,
                            Uk3 = 500,
                            PlayerCount1 = 0,
                            PlayerCount2 = 500
                        },
                        new WorldListEntry
                        {
                            IpAddress = 0x0100007F,
                            Port = 40850,
                            Uk1 = 3,
                            Uk2 = 0,
                            Uk3 = 500,
                            PlayerCount1 = 0,
                            PlayerCount2 = 500
                        },
                        new WorldListEntry
                        {
                            IpAddress = 0x0100007F,
                            Port = 40850,
                            Uk1 = 3,
                            Uk2 = 0,
                            Uk3 = 500,
                            PlayerCount1 = 0,
                            PlayerCount2 = 500
                        },
                        new WorldListEntry
                        {
                            IpAddress = 0x0100007F,
                            Port = 40850,
                            Uk1 = 3,
                            Uk2 = 0,
                            Uk3 = 500,
                            PlayerCount1 = 0,
                            PlayerCount2 = 500
                        }
                    ]
                });
                break;
            case CS_BR_WORLDINFO_REQ:
                await SendPacketAsync(new CS_BR_WORLDINFO_ACK
                {
                    WorldInfoEntries =
                    [
                        new WorldInfoEntry
                        {
                            Name = "Newbie Channel",
                            Kills = 0,
                            Deaths = 10000,
                            GP = 100,
                            Gold = 100
                        },
                        new WorldInfoEntry
                        {
                            Name = "Expert Channel",
                            Kills = 0,
                            Deaths = 10000,
                            GP = 100,
                            Gold = 100
                        },
                        new WorldInfoEntry
                        {
                            Name = "Free Channel 1",
                            Kills = 0,
                            Deaths = 10000,
                            GP = 70,
                            Gold = 70
                        },
                        new WorldInfoEntry
                        {
                            Name = "Free Channel 2",
                            Kills = 0,
                            Deaths = 10000,
                            GP = 70,
                            Gold = 70
                        },
                        new WorldInfoEntry
                        {
                            Name = "Free Channel 3",
                            Kills = 0,
                            Deaths = 10000,
                            GP = 70,
                            Gold = 70
                        },
                        new WorldInfoEntry
                        {
                            Name = "Free Channel 4",
                            Kills = 0,
                            Deaths = 10000,
                            GP = 70,
                            Gold = 70
                        },
                        new WorldInfoEntry
                        {
                            Name = "Free Channel 5",
                            Kills = 0,
                            Deaths = 10000,
                            GP = 70,
                            Gold = 70
                        },
                        new WorldInfoEntry
                        {
                            Name = "Free Channel 6",
                            Kills = 0,
                            Deaths = 10000,
                            GP = 70,
                            Gold = 70
                        },
                        new WorldInfoEntry
                        {
                            Name = "Pride Battle",
                            Kills = 0,
                            Deaths = 10000,
                            GP = 100,
                            Gold = 100
                        },
                        new WorldInfoEntry
                        {
                            Name = "Pride Ladder",
                            Kills = 0,
                            Deaths = 10000,
                            GP = 0,
                            Gold = 0
                        },
                        new WorldInfoEntry
                        {
                            Name = "Van helsing",
                            Kills = 0,
                            Deaths = 10000,
                            GP = 0,
                            Gold = 0
                        },
                        new WorldInfoEntry
                        {
                            Name = "Team Destruction",
                            Kills = 0,
                            Deaths = 10000,
                            GP = 0,
                            Gold = 0
                        },
                        new WorldInfoEntry
                        {
                            Name = "Pride Battle 1",
                            Kills = 0,
                            Deaths = 10000,
                            GP = 0,
                            Gold = 0
                        },
                        new WorldInfoEntry
                        {
                            Name = "Pride Battle 2",
                            Kills = 0,
                            Deaths = 10000,
                            GP = 0,
                            Gold = 0
                        },
                        new WorldInfoEntry
                        {
                            Name = "Tester Channel 1",
                            Kills = 0,
                            Deaths = 10000,
                            GP = 0,
                            Gold = 0
                        },
                        new WorldInfoEntry
                        {
                            Name = "Tester Channel 2",
                            Kills = 0,
                            Deaths = 10000,
                            GP = 0,
                            Gold = 0
                        },
                        new WorldInfoEntry
                        {
                            Name = "empty",
                            Kills = 0,
                            Deaths = 10000,
                            GP = 0,
                            Gold = 0
                        },
                        new WorldInfoEntry
                        {
                            Name = "empty",
                            Kills = 0,
                            Deaths = 10000,
                            GP = 0,
                            Gold = 0
                        },
                        new WorldInfoEntry
                        {
                            Name = "empty",
                            Kills = 0,
                            Deaths = 10000,
                            GP = 0,
                            Gold = 0
                        },
                        new WorldInfoEntry
                        {
                            Name = "empty",
                            Kills = 0,
                            Deaths = 10000,
                            GP = 0,
                            Gold = 0
                        }
                    ]
                });
                break;
            case CS_BR_RELAYLIST_REQ:
                await SendPacketAsync(new CS_BR_RELAYLIST_ACK
                {
                    // Seems to be an outdated length field.
                    Padding = 20,
                    // 20 entries of the same relay.
                    Relays = Enumerable.Range(0, 20).Select(relayId => new RelayEntry
                    {
                        Id = (byte)relayId,
                        Address = 0x0100007F,
                        Port = 16540,
                        Padding = 0
                    }).ToArray()
                });
                break;
            default:
                Logger.Warning("Unhandled packet {PacketId}", id);
                break;
        }
    }
}