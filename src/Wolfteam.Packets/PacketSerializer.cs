// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-24.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Serilog;
using Wolfteam.Packets.Attributes;

namespace Wolfteam.Packets;

public static class PacketSerializer
{
    private static readonly ILogger Logger = Log.ForContext(typeof(PacketSerializer));

    private static readonly Dictionary<PacketId, Type> PacketIdToType;
    private static readonly Dictionary<Type, PacketId> PacketTypeToId;

    static PacketSerializer()
    {
        PacketIdToType = new Dictionary<PacketId, Type>();
        PacketTypeToId = new Dictionary<Type, PacketId>();
        
        var packetTypes = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(x => x.GetInterfaces().Contains(typeof(IWolfPacket)))
            .ToList();

        foreach (var packetType in packetTypes)
        {
            var wolfteamPacketAttribute = packetType.GetCustomAttribute<WolfteamPacketAttribute>();
            if (wolfteamPacketAttribute == null)
            {
                continue;
            }

            if (!PacketIdToType.TryAdd(wolfteamPacketAttribute.Id, packetType))
            {
                Logger.Warning("Duplicate packet id {PacketId} for {PacketType}", wolfteamPacketAttribute.Id, packetType.Name);
            }
            
            if (!PacketTypeToId.TryAdd(packetType, wolfteamPacketAttribute.Id))
            {
                Logger.Warning("Duplicate packet type {PacketType} for {PacketId}", packetType.Name, wolfteamPacketAttribute.Id);
            }
        }
    }

    public static bool TryGetId(IWolfPacket packet, out PacketId packetId)
    {
        return PacketTypeToId.TryGetValue(packet.GetType(), out packetId);
    }
    
    public static bool TryDeserialize(PacketId packetId, ClientVersion version, ReadOnlySpan<byte> payload, [NotNullWhen(true)] out IWolfPacket? packet)
    {
        if (!PacketIdToType.TryGetValue(packetId, out var objType))
        {
            var packetIdShort = (ushort)packetId;
            
            Logger.Warning("Missing class for packet id {PacketId}", packetId);
            
            if (!Enum.IsDefined(typeof(PacketId), packetIdShort))
            {
                Logger.Verbose("Tip: find with \"{A:x2} {B:x2} 00 00\"", packetIdShort & 0xFF, packetIdShort >> 8);
            }

            packet = null;
            return false;
        }

        packet = (IWolfPacket)Activator.CreateInstance(objType)!;
        
        var reader = new SpanReader(payload);
        var result = packet.Deserialize(version, ref reader);
        if (result && reader.Remaining > 0 && payload[^reader.Remaining] != 0)
        {
            Logger.Warning("Packet {PacketType} has remaining bytes: {RemainingBytes}", packet.GetType().Name, reader.Remaining);
        }
        
        return result;
    }
}