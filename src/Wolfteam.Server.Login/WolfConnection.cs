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
    private readonly Pipe _pipe;
    private readonly Aes _cryptoStatic;
    private readonly Aes _cryptoAuth;

    private bool _disposed;
    private Task? _incomingTask;

    public WolfConnection(Guid id, Socket client)
    {
        _id = id;
        _client = client;
        _pipe = new Pipe(new PipeOptions());
        _cryptoStatic = Aes.Create();
        _cryptoStatic.Key = StaticKey;
        _cryptoAuth = Aes.Create();
    }

    public void Start()
    {
        if (_incomingTask != null)
        {
            throw new InvalidOperationException("Receive task is already running");
        }
        
        var receiveTask = ReceiveAsync();
        var handleTask = ProcessAsync();
        
        _incomingTask = Task.WhenAll(receiveTask, handleTask).ContinueWith(_ =>
        {
            Dispose();
        });
    }

    private async Task ReceiveAsync()
    {
        try
        {
            while (true)
            {
                var memory = _pipe.Writer.GetMemory(MinimumBufferSize);
                var bytesRead = await _client.ReceiveAsync(memory, SocketFlags.None);
                if (bytesRead == 0)
                {
                    Logger.Information("Client {ConnectionId} disconnected", _id);
                    break;
                }

                _pipe.Writer.Advance(bytesRead);

                var result = await _pipe.Writer.FlushAsync();
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

            await _pipe.Writer.CompleteAsync();
            
            Disconnect();
        }
    }

    private async Task ProcessAsync()
    {
        var reader = _pipe.Reader;

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
            case 4880:
            {
                if (!LoginRequest.TryParseStatic(_cryptoStatic, ref reader, out var username, out var magic))
                {
                    Logger.Warning("Invalid username data");
                    return;
                }

                Logger.Information("  Username {Username}", username);
                Logger.Information("  Magic    {Magic}", magic);
                
                // TODO: Retrieve password hash from database.
                var passwordHash = MD5.HashData("qqq"u8.ToArray());
                
                _cryptoAuth.Key = LoginRequest.CreateAuthKey(username, passwordHash, magic);
                
                if (!LoginRequest.TryDecryptAuth(_cryptoAuth, ref reader, out var password, out var version))
                {
                    Logger.Warning("Invalid password data");
                    return;
                }
                
                Logger.Information("  Password {Password}", password);
                Logger.Information("  Version  {Version}", version);
                break;
            }
        }
        
        if (!reader.Completed)
        {
            Logger.Warning("Packet {PacketId} has {Bytes} bytes remaining", packetId, reader.Remaining);
        }

        await Task.Delay(10);
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
        _pipe.Writer.CancelPendingFlush();
        _pipe.Reader.CancelPendingRead();
        
        Disconnect();
    }
}