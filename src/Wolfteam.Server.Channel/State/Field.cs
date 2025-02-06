// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-29.

using System.Buffers.Binary;
using System.Collections.Concurrent;
using Wolfteam.Packets;
using Wolfteam.Packets.Channel;
using Wolfteam.Packets.Channel.Data;

namespace Wolfteam.Server.Channel.State;

public class Field
{
    private readonly ConcurrentDictionary<byte, FieldChar> _chars;
    private readonly SemaphoreSlim _fieldLock;

    public Field(ushort id, CS_FD_CREATE_REQ request)
    {
        _chars = new ConcurrentDictionary<byte, FieldChar>();
        _fieldLock = new SemaphoreSlim(1, 1);
        
        OwnerSlot = 0;
        
        Id = id;
        Title = request.Title ?? throw new ArgumentNullException(nameof(request.Title));
        Password = request.Password ?? throw new ArgumentNullException(nameof(request.Password));
        MapId = request.MapId;
        Mode = request.Mode;
        Wins = request.Wins;
        Time = request.Time;
        Mission = request.Mission;
        FlagTresspass = request.FlagTresspass;
        FlagHero = request.FlagHero;
        FlagHeavy = request.FlagHeavy;
        PlayerLimit = request.PlayerLimit;
        IsPowerRoom = 0;
        ClassMax = request.ClassMax;
        ClassMin = request.ClassMin;
    }

    public byte OwnerSlot { get; private set; }
    
    public ushort Id { get; }
    public string Title { get; set; }
    public string Password { get; set; }
    public byte MapId { get; set; }
    public uint Mode { get; set; }
    public byte Wins { get; set; }
    public ushort Time { get; set; }
    public ushort Mission { get; set; }
    public byte FlagTresspass { get; set; }
    public byte FlagHero { get; set; }
    public ushort FlagHeavy { get; set; }
    public byte PlayerLimit { get; set; }
    public byte PlayerCount => (byte)_chars.Count;
    public byte IsPowerRoom { get; set; }
    public byte ClassMax { get; set; }
    public byte ClassMin { get; set; }

    private (int red, int blue) GetTeamCount()
    {
        var red = 0;
        var blue = 0;
        
        foreach (var (_, value) in _chars)
        {
            if (value.Team == FieldCharTeam.Red)
            {
                red++;
            }
            else if (value.Team == FieldCharTeam.Blue)
            {
                blue++;
            }
        }

        return (red, blue);
    }

    /// <summary>
    ///     Find an empty slot in the field.
    ///     Returns null if no slot is available.
    /// </summary>
    private byte? FindSlot()
    {
        byte? slot = null;
            
        for (byte i = 0; i < PlayerLimit; i++)
        {
            if (_chars.ContainsKey(i))
            {
                continue;
            }
            
            slot = i;
            break;
        }

        return slot;
    }

    /// <summary>
    ///     Find an empty position in the team.
    ///     Returns null if no position is available.
    /// </summary>
    private byte? FindPosition(FieldCharTeam team)
    {
        var takenPositions = _chars.Values
            .Where(x => x.Team == team)
            .Select(x => x.Position)
            .ToHashSet();
        
        byte? position = null;

        for (byte i = 0; i < PlayerLimit / 2; i++)
        {
            if (takenPositions.Contains(i))
            {
                continue;
            }
            
            position = i;
            break;
        }

        return position;
    }

    public async Task<bool> AddPlayer(PlayerSession player, FieldAddReason reason)
    {
        await _fieldLock.WaitAsync();

        try
        {
            // Find an empty slot.
            var slot = FindSlot();
            if (slot == null)
            {
                return false;
            }
            
            // Find team to place into.
            var (red, blue) = GetTeamCount();
            var team = red <= blue ? FieldCharTeam.Red : FieldCharTeam.Blue;
            var teamPosition = FindPosition(team);
            if (teamPosition == null)
            {
                return false;
            }
            
            // Add the player to the field.
            var fieldChar = new FieldChar(this, slot.Value, player, team, teamPosition.Value);
            
            if (!_chars.TryAdd(slot.Value, fieldChar))
            {
                return false;
            }
            
            // Set as owner.
            if (_chars.Count == 1)
            {
                OwnerSlot = slot.Value;
            }
            
            // Create association.
            player.FieldChar = fieldChar;
            
            // Proper response.
            if (reason == FieldAddReason.Create)
            {
                // Acknowledge creation to player.
                await SendCreateAckAsync(fieldChar);
            } 
            else if (reason == FieldAddReason.Enter)
            {
                // Acknowledge entry to entire room.
                await SendEnterAckAsync(fieldChar);
            }
            
            // Sync characters to the new player.
            await SendCharsAsync(fieldChar);
            return true;
        }
        finally
        {
            _fieldLock.Release();
        }
    }
    
    public async Task RemovePlayer(byte slot)
    {
        await _fieldLock.WaitAsync();
        
        try
        {
            if (!_chars.TryRemove(slot, out var fieldChar))
            {
                return;
            }

            // Remove association on player.
            fieldChar.Player.FieldChar = null;
            
            // Let the player know.
            var packet = new CS_FD_EXIT_ACK
            {
                Slot = slot
            };
            
            await fieldChar.Player.Connection.SendPacketAsync(packet);
            
            // Let the other players know.
            await SendPacketAsync(packet);
        }
        finally
        {
            _fieldLock.Release();
        }
    }
    
    /// <summary>
    ///     Change the team of a player.
    /// </summary>
    /// <param name="slot">Slot of the player.</param>
    /// <returns>Whether an acknowledgement was sent to the client.</returns>
    public async Task<bool> ChangeTeam(byte slot)
    {
        await _fieldLock.WaitAsync();

        try
        {
            if (!_chars.TryGetValue(slot, out var fieldChar))
            {
                return false;
            }
            
            // Move player.
            var teamTarget = fieldChar.Team == FieldCharTeam.Red ? FieldCharTeam.Blue : FieldCharTeam.Red;
            var teamPos = FindPosition(teamTarget);
            if (teamPos == null)
            {
                await fieldChar.Player.Connection.SendPacketAsync(new CS_FD_CHANGETEAM_ACK(), ErrorCode.S182_NotEnoughSpaceForTeamChange);
                return true;
            }
            
            fieldChar.Team = teamTarget;
            fieldChar.Position = teamPos.Value;
            
            await SendPacketAsync(new CS_FD_CHANGETEAM_ACK
            {
                Slot = slot,
                Team = (byte)fieldChar.Team,
                Position = fieldChar.Position
            });

            return true;
        }
        finally
        {
            _fieldLock.Release();
        }
    }

    public async Task StartUdpAsync(FieldChar fChar)
    {
        await SendPacketAsync(new CS_FD_UDPSTART_ACK
        {
            Slot = fChar.Slot
        });
    }

    public async Task ReadyUdpAsync(FieldChar fChar, byte ready)
    {
        await SendPacketAsync(new CS_FD_UDPREADY_ACK
        {
            Slot = fChar.Slot,
            Ready = ready
        });
    }

    public async Task StartGameAsync(FieldChar sender)
    {
        if (sender.Slot != OwnerSlot)
        {
            await sender.Player.Connection.SendPacketAsync(new CS_FD_STARTGAME_ACK(), ErrorCode.S203_RoomMastersOnly);
            return;
        }
        
        // TODO: Improve this, also change slot statuses (See CS_FD_CHANGESLOTSTATUS_ACK).
        foreach (var (_, value) in _chars)
        {
            await value.UpdateStatus(3);
        }
        
        await SendPacketAsync(new CS_FD_STARTGAME_ACK
        {
            Uk1 = 0,
            Uk2 = 0
        });
    }

    public async Task StartRoundAsync(FieldChar sender)
    {
        // Status 4 makes player appear in the ingame leaderboard.
        // Important to send before starting the round for player, otherwise loadout selection won't appear.
        await sender.UpdateStatus(4);
        
        // Start round.
        await sender.Player.Connection.SendPacketAsync(new CS_FD_STARTROUND_ACK
        {
            Uk1 = 1,
            Uk2 = 600,
            Uk3 = 0,
            Uk4 = 0,
            Uk5 = []
        });
    }
    
    private async Task SendCreateAckAsync(FieldChar newChar)
    {
        await newChar.Player.Connection.SendPacketAsync(new CS_FD_CREATE_ACK
        {
            Unused = 0,
            Uk2 = 0,
            Uk3 = 0,
            Uk4 = 0x3D
        });
    }

    private async Task SendEnterAckAsync(FieldChar newChar)
    {
        var enterAck = new CS_FD_ENTER_ACK
        {
            Slot = newChar.Slot,
            Unused3 = 0,
            Team = (byte)newChar.Team,
            Position = newChar.Position,
            ConnectionId = newChar.Player.SessionId,
            Uk6 = 0,
            Uk7 = 0,
            Class = 61,
            Uk9 = 0,
            Uk10 = 0,
            Uk11 = string.Empty,
            NickName = newChar.Player.Username,
            Pride = "Morning",
            Uk14 = string.Empty,
            Uk15 = 0,
            Uk16 = [
                new FieldCharEntry4_Uk16
                {
                    Uk1 = 0,
                    Uk10 = 2001,
                },
                new FieldCharEntry4_Uk16
                {
                    Uk1 = 1,
                    Uk10 = 2001,
                },
                new FieldCharEntry4_Uk16
                {
                    Uk1 = 2,
                    Uk10 = 2001,
                },
                new FieldCharEntry4_Uk16
                {
                    Uk1 = 3,
                    Uk10 = 2001,
                }
            ],
            Uk17 = [
                30006,
                30006,
            ],
            Uk18 = [
                30005,
                30005,
            ],
            Uk19 = 0,
            Uk20 = 0x33,
            Uk21 = 0,
            Uk22 = 0,
            Uk27 = [
                new FieldCharEntry4_Sub
                {
                    Uk1 = 0,
                    Uk2 = 0
                },
                new FieldCharEntry4_Sub
                {
                    Uk1 = 1,
                    Uk2 = 0
                },
                new FieldCharEntry4_Sub
                {
                    Uk1 = 2,
                    Uk2 = 0
                }
            ],
        };
        
        var udp = newChar.Player.UdpConnectionDetails;
        if (udp.RemoteEndPoint != null)
        {
            var remoteIp = udp.RemoteEndPoint.Address.GetAddressBytes();
            var remotePort = (ushort)udp.RemoteEndPoint.Port;
                
            enterAck.RemoteIp = BinaryPrimitives.ReadUInt32LittleEndian(remoteIp);
            enterAck.RemotePort = remotePort;
        }

        if (udp.LocalEndPoint != null)
        {
            var localIp = udp.LocalEndPoint.Address.GetAddressBytes();
            var localPort = (ushort)udp.LocalEndPoint.Port;
                
            enterAck.LocalIp = BinaryPrimitives.ReadUInt32LittleEndian(localIp);
            enterAck.LocalPort = localPort;
        }
        
        await SendPacketAsync(enterAck);
    }
    
    /// <summary>
    ///     Syncs the characters to the new client.
    /// </summary>
    private async Task SendCharsAsync(FieldChar target)
    {
        var chars = new FieldCharEntry4[_chars.Count];
        var charIndex = 0;
        
        foreach (var (key, value) in _chars)
        {
            var entry = new FieldCharEntry4
            {
                Slot = key,
                Team = (byte)value.Team, // 0 = ?, 1 = Red, 2 = Blue
                Position = value.Position,

                Uk4 = 0, // 0 = Play, 1 = Ready
                ConnectionId = value.Player.SessionId,
                Uk6 = 0,
                Uk7 = 0,
                Class = 61,
                Uk9 = 0,
                Uk10 = 0,
                UkAA = string.Empty,
                NickName = value.Player.Username,
                Pride = "Morning",
                Uk11 = string.Empty,
                Uk11_1 = 0,
                Uk16 = 1, // 0 = Play, 1 = Ready
                Uk17 = [
                    new FieldCharEntry4_Uk16
                    {
                        Uk1 = 0,
                        Uk10 = 2001,
                    },
                    new FieldCharEntry4_Uk16
                    {
                        Uk1 = 1,
                        Uk10 = 2001,
                    },
                    new FieldCharEntry4_Uk16
                    {
                        Uk1 = 2,
                        Uk10 = 2001,
                    },
                    new FieldCharEntry4_Uk16
                    {
                        Uk1 = 3,
                        Uk10 = 2001,
                    }
                ],
                Uk18 = [
                    30006,
                    30006,
                ],
                Uk19 = [
                    30005,
                    30005,
                ],
                Uk20 = 0,
                Uk21 = 0x33,
                Uk22 = [
                    new FieldCharEntry4_Sub
                    {
                        Uk1 = 0,
                        Uk2 = 0
                    },
                    new FieldCharEntry4_Sub
                    {
                        Uk1 = 1,
                        Uk2 = 0
                    },
                    new FieldCharEntry4_Sub
                    {
                        Uk1 = 2,
                        Uk2 = 0
                    }
                ]
            };

            var udp = value.Player.UdpConnectionDetails;
            if (udp.RemoteEndPoint != null)
            {
                var remoteIp = udp.RemoteEndPoint.Address.GetAddressBytes();
                var remotePort = (ushort)udp.RemoteEndPoint.Port;
                
                entry.RemoteIp = BinaryPrimitives.ReadUInt32LittleEndian(remoteIp);
                entry.RemotePort = remotePort;
            }

            if (udp.LocalEndPoint != null)
            {
                var localIp = udp.LocalEndPoint.Address.GetAddressBytes();
                var localPort = (ushort)udp.LocalEndPoint.Port;
                
                entry.LocalIp = BinaryPrimitives.ReadUInt32LittleEndian(localIp);
                entry.LocalPort = localPort;
            }
            
            chars[charIndex++] = entry;
        }

        var packet = new CS_FD_CHARS_ACK
        {
            Uk1 = 1, // 1 Allows invite and inventory stuff, 0 disables it. (I thought) 
                     // 1 Allows start button to be pressed, 0 disables its functionality
            OwnerCharId = OwnerSlot,
            FieldId = Id, // Visually it's + 1.
            Title = Title,
            Password = Password,
            MapId = MapId,
            Mode = Mode,
            Wins = Wins,
            UnusedArray = 0,
            Time = Time,
            Mission = Mission,
            FlagTresspass = FlagTresspass, // Effect on "Trespass" and "Observe"
            FlagHero = FlagHero, // Effect on "Hero" and "Switch offense and defense"
            FlagHeavy = FlagHeavy,
            PlayerLimit = PlayerLimit,
            Uk16 = PlayerCount,
            Uk17 = 0,
            Uk18 = 0,
            IsPowerRoom = IsPowerRoom,
            Chars = chars,
            ClassMax = ClassMax,
            ClassMin = ClassMin
        };
        
        // Send to new player in the field.
        await target.Player.Connection.SendPacketAsync(packet);
    }
    
    /// <summary>
    ///     Send a packet to all players in the field.
    /// </summary>
    public async Task SendPacketAsync(IWolfPacket packet)
    {
        foreach (var (_, value) in _chars)
        {
            await value.Player.Connection.SendPacketAsync(packet);
        }
    }
}