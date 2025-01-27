namespace Wolfteam.Packets.Channel.Data;

public partial class CharInfoEntry : IWolfPacket
{
    public byte Uk1 { get; set; }
    
    public byte Uk2 { get; set; }
    
    public uint Uk3 { get; set; }
}