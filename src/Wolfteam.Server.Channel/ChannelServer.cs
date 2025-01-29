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
    private static readonly ClientVersion Version = ClientVersion.IS_854;
    
    private readonly int _port;
    private readonly UdpClient _udp;
    private readonly ChannelState _state;
    
    private Task? _udpTask;
    
    public ChannelServer(int port) : base(Logger, port)
    {
        _port = port;
        _udp = new UdpClient();
        _state = new ChannelState();
    }

    public override void Listen()
    {
        base.Listen();
        
        _udp.Client.Bind(new IPEndPoint(IPAddress.Any, _port));
        _udpTask = ListenUdpAsync();
    }

    protected override ChannelConnection OnConnectionAccepted(Guid clientId, Socket socket)
    {
        return new ChannelConnection(clientId, socket, Version, _state);
    }

    protected override Task OnConnectionClosed(WolfConnection connection)
    {
        if (connection is ChannelConnection channelConnection && channelConnection.Player != null)
        {
            if (!_state.RemovePlayer(channelConnection.Player.SessionId))
            {
                Logger.Error("Failed to remove player with SessionId: {SessionId}", channelConnection.Player.SessionId);
            }
        }
        
        return Task.CompletedTask;
    }

    private async Task ListenUdpAsync()
    {
        try
        {
            while (true)
            {
                var data = await _udp.ReceiveAsync();

                Log.Information("Received UDP: {Amount} bytes from {RemoteEndpoint}, {Data}", 
                    data.Buffer.Length, 
                    data.RemoteEndPoint,
                    data.Buffer);

                var reader = new SpanReader(data.Buffer);

                if (!reader.TryReadU16(out var id))
                {
                    continue;
                }
                
                // Logger.Information("UDP Received Id: {Id}", id);

                // Ping.
                // if (id == 65484)
                // {
                //     await AttemptResponseTwoAsync(data.RemoteEndPoint);
                //     continue;
                // }
                
                if (!reader.TryReadU16(out var length))
                {
                    continue;
                }

                var packetId = (PacketId)id;

                Log.Information("UDP Received Id: {PacketId}, Length: {Length}", packetId, length);

                switch (packetId)
                {
                    case PacketId.CS_UD_UDPADDR_REQ:
                    {
                        var udpAddrReq = new CS_UD_UDPADDR_REQ();
                        if (!udpAddrReq.Deserialize(0, ref reader))
                        {
                            Log.Error("UDP Failed to deserialize");
                            break;
                        }

                        Log.Information("UDP Received: {@UdpaddrReq}", udpAddrReq);

                        // Find the related TCP connection.
                        if (!_state.TryGetPlayer(udpAddrReq.SessionId, out var player))
                        {
                            Log.Error("UDP Failed to find player with SessionId: {SessionId}", udpAddrReq.SessionId);
                            break;
                        }
                        
                        if (player.SessionKey != udpAddrReq.SessionKey)
                        {
                            Log.Error("UDP SessionKey mismatch: {SessionKey:X4} != {SessionKey2:X4}", player.SessionKey, udpAddrReq.SessionKey);
                            break;
                        }

                        // Update channel UDP details.
                        player.UdpConnectionDetails.RemoteEndPoint = data.RemoteEndPoint;
                        
                        await player.Connection.HandlePacketAsync(PacketId.CS_UD_UDPADDR_REQ, udpAddrReq);
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
        finally
        {
            Logger.Information("UDP Task finished");
        }
    }

    // private async Task AttemptResponseAsync(IPEndPoint remoteEndPoint)
    // {
    //     var blowfish = new Blowfish(BlowfishMode.Relay);
    //     var header = new byte[8];
    //
    //     header[0] = 0x11;
    //     header[1] = 0x22;
    //     header[2] = 0x33;
    //     header[3] = 0x44;
    //     header[4] = 0xAA;
    //     header[5] = 0xBB;
    //     header[6] = 0xCC;
    //     header[7] = 0xDD;
    //     
    //     // Unknown.
    //     header[1] = 0xFF;
    //     
    //     BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(2), 0);
    //     BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(4), 0);
    //     
    //     // Calculate checksum.
    //     header[0] = (byte)(header[1] ^ header[2] ^ header[3] ^ header[4] ^ header[5] ^ header[6] ^ header[7] ^ 0x4C);
    //     
    //     blowfish.Encrypt(header);
    //     
    //     Logger.Debug("Sending {Data} to {RemoteEndPoint}", header, remoteEndPoint);
    //     
    //     await _udp.SendAsync(header, remoteEndPoint);
    // }
    //
    // private async Task AttemptResponseTwoAsync(IPEndPoint remoteEndPoint)
    // {
    //     var blowfish = new Blowfish(BlowfishMode.Default);
    //     var headerLen = 8;
    //     var header = new byte[headerLen];
    //
    //     header[0] = 0x21; // Protocol: Options: 0x62
    //     header[1] = 0x00; // Checksum
    //     header[2] = 0x00; // Checksum
    //     header[3] = 0x44; // byte ?
    //     header[4] = 0xAA; // byte ?
    //     header[5] = 0x00;
    //
    //     if (header.Length > 6)
    //     {
    //         ushort checksum = 0;
    //
    //         var pos = 6;
    //         var reader = header.AsSpan();
    //         
    //         if ((header.Length - 5) / 2 >= 2)
    //         {
    //             do
    //             {
    //                 checksum += BinaryPrimitives.ReadUInt16LittleEndian(reader.Slice(pos));
    //                 checksum += BinaryPrimitives.ReadUInt16LittleEndian(reader.Slice(pos + 2));
    //                 pos += 4;
    //             } while (pos < header.Length - 2);
    //         }
    //
    //         if (pos < header.Length)
    //         {
    //             checksum += BinaryPrimitives.ReadUInt16LittleEndian(reader.Slice(pos));
    //         }
    //         
    //         BinaryPrimitives.WriteUInt16LittleEndian(reader.Slice(1), checksum);
    //     }
    //     
    //     // Xor.
    //     for (var i = 1; i < ((header.Length + 3) >> 2); ++i)
    //     {
    //         var l = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(4 * i));
    //         var r = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(4 * i - 4));
    //
    //         // Reverse the transformation: l ^ ~(32 * r)
    //         BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(4 * i), l ^ ~(32 * r));
    //     }
    //     
    //     // Xor decrypt.
    //     // for (var i = ((header.Length + 3) >> 2) - 1; i > 0; --i)
    //     // {
    //     //     var l = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(4 * i));
    //     //     var r = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(4 * i - 4));
    //     //     
    //     //     BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(4 * i), l ^ ~(32 * r));
    //     // }
    //     
    //     Logger.Debug("Before blowfish {Data}", header);
    //     
    //     blowfish.Encrypt(header);
    //     
    //     Logger.Debug("Sending {Data} to {RemoteEndPoint}", header, remoteEndPoint);
    //     
    //     await _udp.SendAsync(header, remoteEndPoint);
    // }

    public override void Dispose()
    {
        _udp.Close();
        _udp.Dispose();
        
        base.Dispose();
    }
}