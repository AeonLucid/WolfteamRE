using System.Buffers;
using System.Net.Sockets;
using Serilog;

namespace Wolfteam.Server.Broker;

public class BrokerConnection : WolfConnection
{
    private static readonly ILogger Logger = Log.ForContext<BrokerConnection>();
    
    public BrokerConnection(Guid id, Socket client) : base(Logger, id, client)
    {
    }

    protected override ValueTask ProcessPacketAsync(ReadOnlySequence<byte> packet)
    {
        Logger.Debug("Received packet {Packet}", Convert.ToHexStringLower(packet.ToArray()));

        var reader = new PacketReader(packet);
        
        return ValueTask.CompletedTask;
    }
}