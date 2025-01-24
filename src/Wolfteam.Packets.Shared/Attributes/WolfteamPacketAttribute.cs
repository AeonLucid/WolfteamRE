namespace Wolfteam.Packets.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class WolfteamPacketAttribute : Attribute
{
    public WolfteamPacketAttribute(PacketId id)
    {
        Id = id;
    }
    
    public PacketId Id { get; }
}