// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-29.

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
}