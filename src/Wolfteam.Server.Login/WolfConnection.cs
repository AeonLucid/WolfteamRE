using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Security.Cryptography;
using Serilog;
using Wolfteam.Server.Login.Packets;
using static Wolfteam.Server.Login.Constants;

namespace Wolfteam.Server.Login;

public class WolfConnection : IDisposable
{
    private static readonly ILogger Logger = Log.ForContext<WolfConnection>();
    
    private const int MinimumBufferSize = 4096;
    
    private readonly Guid _id;
    private readonly Socket _client;
    private readonly Pipe _recvPipe;
    private readonly Pipe _sendPipe;
    private readonly Aes _cryptoStatic;
    private readonly Aes _cryptoAuth;

    private bool _disposed;
    private Task? connectionTask;

    public WolfConnection(Guid id, Socket client)
    {
        _id = id;
        _client = client;
        _recvPipe = new Pipe(new PipeOptions());
        _sendPipe = new Pipe(new PipeOptions());
        _cryptoStatic = Aes.Create();
        _cryptoStatic.Key = StaticKey;
        _cryptoAuth = Aes.Create();
    }

    public void Start()
    {
        if (connectionTask != null)
        {
            throw new InvalidOperationException("Receive task is already running");
        }
        
        var sendTask = SendAsync();
        var receiveTask = ReceiveAsync();
        var handleTask = ProcessAsync();
        
        connectionTask = Task.WhenAll(sendTask, receiveTask, handleTask).ContinueWith(_ =>
        {
            Dispose();
        });
    }

    private async Task SendAsync()
    {
        var reader = _sendPipe.Reader;

        try
        {
            while (true)
            {
                var result = await reader.ReadAsync();
                var buffer = result.Buffer;

                try
                {
                    // Send buffer to client.
                    var current = buffer;
                    
                    while (current.Length > 0)
                    {
                        var bytes = await _client.SendAsync(current.First);
                        if (bytes == 0)
                        {
                            Logger.Information("Client {ConnectionId} disconnected", _id);
                            return;
                        }

                        current = current.Slice(bytes);
                    }

                    if (result.IsCompleted)
                    {
                        break;
                    }
                }
                finally
                {
                    reader.AdvanceTo(buffer.End);
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Error sending data to client {ConnectionId}", _id);
        }
        finally
        {
            Logger.Information("Client {ConnectionId} stopped send loop", _id);
            
            await reader.CompleteAsync();
            
            Disconnect();
        }
    }

    private async Task ReceiveAsync()
    {
        try
        {
            while (true)
            {
                var memory = _recvPipe.Writer.GetMemory(MinimumBufferSize);
                var bytesRead = await _client.ReceiveAsync(memory, SocketFlags.None);
                if (bytesRead == 0)
                {
                    Logger.Information("Client {ConnectionId} disconnected", _id);
                    break;
                }

                _recvPipe.Writer.Advance(bytesRead);

                var result = await _recvPipe.Writer.FlushAsync();
                if (result.IsCompleted)
                {
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Error receiving data from client {ConnectionId}", _id);
        }
        finally
        {
            Logger.Information("Client {ConnectionId} stopped receive loop", _id);

            await _recvPipe.Writer.CompleteAsync();
            
            Disconnect();
        }
    }

    private async Task ProcessAsync()
    {
        var reader = _recvPipe.Reader;

        try
        {
            while (true)
            {
                var result = await reader.ReadAsync();
                var buffer = result.Buffer;

                try
                {
                    while (TryReadPacket(ref buffer, out var packet))
                    {
                        await ProcessPacketAsync(packet);
                    }

                    if (result.IsCompleted)
                    {
                        if (buffer.Length > 0)
                        {
                            throw new InvalidDataException("Incomplete message.");
                        }
                        
                        break;
                    }
                }
                finally
                {
                    reader.AdvanceTo(buffer.Start, buffer.End);
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Error processing data from client {ConnectionId}", _id);
        }
        finally
        {
            Logger.Information("Client {ConnectionId} stopped process loop", _id);
            
            await reader.CompleteAsync();
            
            Disconnect();
        }
    }

    private async ValueTask WriteDataAsync(byte[] packet)
    {
        await _sendPipe.Writer.WriteAsync(packet);
        await _sendPipe.Writer.FlushAsync();
    }
    
    private async ValueTask ProcessPacketAsync(ReadOnlySequence<byte> packet)
    {
        Logger.Debug("Received packet {Packet}", Convert.ToHexStringLower(packet.ToArray()));

        var reader = new PacketReader(packet);

        var packetLen = reader.ReadInt();
        var packetId = reader.ReadShort();

        Logger.Debug("Packet {PacketId} {PacketLen}", packetId, packetLen);
        
        // Very simple packet handler, will implement a better one once more packets added.
        switch (packetId)
        {
            case 0x1310:
            {
                var staticData = reader.ReadBytes(32);
                var authData = reader.ReadBytes(32);
                
                if (!LoginRequest.TryParseStatic(_cryptoStatic, ref staticData, out var username, out var magic))
                {
                    Logger.Warning("Invalid username data");
                    return;
                }

                Logger.Information("  Username {Username}", username);
                Logger.Information("  Magic    {Magic}", magic);

                // TODO: Retrieve password hash from database.
                var passwordHash = MD5.HashData("qqq"u8.ToArray());
                
                _cryptoAuth.Key = LoginRequest.CreateAuthKey(username, passwordHash, magic);
                
                if (!LoginRequest.TryDecryptAuth(_cryptoAuth, ref authData, out var password, out var version))
                {
                    Logger.Warning("Invalid password data");
                    return;
                }
                
                Logger.Information("  Password {Password}", password);
                Logger.Information("  Version  {Version}", version);

                // 0x1312
                // - 0x00 = ? (+8 DWORD, +12 DWORD) or (+8 CHAR, + 24 CHAR)
                // - 0x03 = Can't connect due to too many users
                // - 0x10 = ?
                // - 0x11 = ?
                // - 0x19 = Uncertified ID
                // - 0x30 = Uncertified ID (But also accepts more data)
                // - 0x60 = Incorrect client version
                // - 0x91 = Uncertified ID
                // - 0x95 = Can't connect due to too many users
                
                await WriteDataAsync([
                    0x08, 0x00, 0x00, 0x00,
                    0x12, 0x13, 0x00, 0x00
                ]);
                break;
            }
            case 0x1320:
            {
                Logger.Warning("Unhandled packet {PacketId}, FacebookLogin", packetId);
                break;
            }
            case 0x4000:
            {
                Logger.Warning("Unhandled packet {PacketId}, Pc data?", packetId);
                
                // 0x4001
                // - 0x30 = Game play prohibited
                // await WriteDataAsync([
                //     0x2C, 0x00, 0x00, 0x00,
                //     0x01, 0x40, 0x30, 0x00,
                //     0x00, 0x00, 0x00, 0x00, // tm_sec
                //     0x10, 0x00, 0x00, 0x00, // tm_min
                //     0x00, 0x00, 0x00, 0x00, // tm_hour
                //     0x00, 0x00, 0x00, 0x00, // tm_mday
                //     0x00, 0x00, 0x00, 0x00, // tm_mon
                //     0x00, 0x00, 0x00, 0x00, // tm_year
                //     0x00, 0x00, 0x00, 0x00, // tm_wday
                //     0x00, 0x00, 0x00, 0x00, // tm_yday
                //     0x00, 0x00, 0x00, 0x00, // tm_isdst
                // ]);
                
                // Start game
                await WriteDataAsync([
                    0x08, 0x00, 0x00, 0x00,
                    0x01, 0x40, 0x01, 0x90,
                ]);
                
                // Specify third launch parameter.
                // await WriteDataAsync([
                //     0x0C, 0x00, 0x00, 0x00,
                //     0x01, 0x40, 0x01, 0x90,
                //     0x05, 0x00, 0x00, 0x00,
                // ]);
                break;
            }
            default:
            {
                Logger.Warning("Unhandled packet {PacketId}", packetId);
                break;
            }
        }
        
        // if (!reader.Completed)
        // {
        //     Logger.Warning("Packet {PacketId} has {Bytes} bytes remaining", packetId, reader.Remaining);
        // }
    }
    
    private static bool TryReadPacket(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> packet)
    {
        var reader = new SequenceReader<byte>(buffer);
        
        if (!reader.TryReadLittleEndian(out int packetLen))
        {
            packet = default;
            return false;
        }
        
        // Check if buffer has enough data.
        // The packet length includes the length itself.
        if (buffer.Length < packetLen)
        {
            packet = default;
            return false;
        }

        packet = buffer.Slice(buffer.Start, packetLen);
        buffer = buffer.Slice(packet.End);
        return true;
    }

    private void Disconnect()
    {
        if (_client.Connected)
        {
            try
            {
                _client.Shutdown(SocketShutdown.Both);
            }
            catch (Exception)
            {
                // ignored
            }
        }
        
        try
        {
            _client.Dispose();
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        
        Logger.Verbose("Disposing connection {ConnectionId}", _id);
        
        _disposed = true;
        _sendPipe.Writer.CancelPendingFlush();
        _sendPipe.Reader.CancelPendingRead();
        _recvPipe.Writer.CancelPendingFlush();
        _recvPipe.Reader.CancelPendingRead();
        
        Disconnect();
    }
}