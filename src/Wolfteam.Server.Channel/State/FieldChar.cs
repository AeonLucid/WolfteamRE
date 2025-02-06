// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-29.

using Wolfteam.Packets.Channel;

namespace Wolfteam.Server.Channel.State;

public class FieldChar
{
    public FieldChar(Field field, byte slot, PlayerSession player, FieldCharTeam team, byte position)
    {
        Field = field;
        Slot = slot;
        Player = player;
        Team = team;
        Position = position;
    }

    public Field Field { get; }
    
    public byte Slot { get; }
    
    public PlayerSession Player { get; }
    
    public FieldCharTeam Team { get; set; }
    
    /// <summary>
    ///     Position within the team.
    /// </summary>
    public byte Position { get; set; }

    public byte Status { get; set; }

    public async Task UpdateStatus(byte status)
    {
        Status = status;
        
        await Field.SendPacketAsync(new CS_FD_CHANGESLOTSTATUS_ACK
        {
            Slot = Slot,
            Status = status
        });
    }
}