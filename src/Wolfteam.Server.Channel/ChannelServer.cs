// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-24.

using System.Net;
using System.Net.Sockets;
using Serilog;
using Wolfteam.Packets;
using Wolfteam.Packets.Datagram;

namespace Wolfteam.Server.Channel;

public class ChannelServer : WolfServer<ChannelConnection>
{
    private static readonly ILogger Logger = Log.ForContext<ChannelServer>();
    
    private readonly int _port;
    private readonly UdpClient _udp;
    
    private Task? _udpTask;
    
    public ChannelServer(int port) : base(port)
    {
        _port = port;
        _udp = new UdpClient();
    }

    public override void Listen()
    {
        base.Listen();
        
        _udp.Client.Bind(new IPEndPoint(IPAddress.Any, _port));
        _udpTask = ListenUdpAsync();
    }

    private async Task ListenUdpAsync()
    {
        try
        {
            while (true)
            {
                var data = await _udp.ReceiveAsync();
                
                Log.Information("Received UDP: {Amount} bytes from {RemoteEndpoint}", data.Buffer.Length, data.RemoteEndPoint);
                
                var reader = new SpanReader(data.Buffer);
            
                if (!reader.TryReadU16(out var id))
                {
                    return;
                }

                if (!reader.TryReadU16(out var length))
                {
                    return;
                }
            
                Log.Information("UDP Received Id: {Id}, Length: {Length}", id, length);

                var packetId = (PacketId)id;
                
                switch (packetId)
                {
                    case PacketId.CS_UD_UDPADDR_REQ:
                    {
                        var udpaddrReq = new CS_UD_UDPADDR_REQ();
                        if (!udpaddrReq.Deserialize(0, ref reader))
                        {
                            Log.Error("UDP Failed to deserialize");
                            break;
                        }
                        
                        Log.Information("UDP Received: {@UdpaddrReq}", udpaddrReq);
                        
                        // Find the related TCP connection.
                        var connection = Connections.FirstOrDefault();
                        if (connection.Value is not ChannelConnection channelConnection)
                        {
                            Log.Error("UDP Failed to find related TCP connection");
                            break;
                        }
                        
                        await channelConnection.HandlePacketAsync(PacketId.CS_UD_UDPADDR_REQ, udpaddrReq);
                        break;
                    }

                    default:
                    {
                        Logger.Warning("Unknown UDP packet id: {Id}", id);
                        break;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Error accepting UDP");
        }
    }

    public override void Dispose()
    {
        _udp.Close();
        _udp.Dispose();
        
        base.Dispose();
    }
}