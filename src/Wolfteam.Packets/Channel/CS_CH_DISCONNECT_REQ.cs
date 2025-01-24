// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-22.

using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets.Channel;

[WolfteamPacket(PacketId.CS_CH_DISCONNECT_REQ)]
public class CS_CH_DISCONNECT_REQ : IWolfPacket
{
    public byte[]? Unknown { get; set; }

    public int Size(ClientVersion version)
    {
        throw new NotImplementedException();
    }

    public void Serialize(ClientVersion version, ref SpanWriter writer)
    {
        throw new NotImplementedException();
    }

    public bool Deserialize(ClientVersion version, ref SpanReader reader)
    {
        if (!reader.TryReadBytes(0x100, out var unknown))
        {
            return false;
        }
        
        Unknown = unknown;
        return true;
    }
}