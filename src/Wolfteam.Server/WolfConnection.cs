using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using Serilog;

namespace Wolfteam.Server;

public abstract class WolfConnection : IDisposable
{
    private const int MinimumBufferSize = 4096;

    private readonly ILogger _logger;
    private readonly Guid _id;
    private readonly Socket _client;
    private readonly Pipe _recvPipe;
    private readonly Pipe _sendPipe;

    private bool _disposed;
    private Task? connectionTask;

    public WolfConnection(ILogger logger, Guid id, Socket client)
    {
        _logger = logger;
        _id = id;
        _client = client;
        _recvPipe = new Pipe(new PipeOptions());
        _sendPipe = new Pipe(new PipeOptions());
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
                            _logger.Information("Client {ConnectionId} disconnected", _id);
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
            _logger.Error(e, "Error sending data to client {ConnectionId}", _id);
        }
        finally
        {
            _logger.Information("Client {ConnectionId} stopped send loop", _id);
            
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
                    _logger.Information("Client {ConnectionId} disconnected", _id);
                    break;
                }
                
                _logger.Verbose("Received data from client {ConnectionId}: {Data}", _id, Convert.ToHexStringLower(memory.Span[..bytesRead]));

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
            _logger.Error(e, "Error receiving data from client {ConnectionId}", _id);
        }
        finally
        {
            _logger.Information("Client {ConnectionId} stopped receive loop", _id);

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
            _logger.Error(e, "Error processing data from client {ConnectionId}", _id);
        }
        finally
        {
            _logger.Information("Client {ConnectionId} stopped process loop", _id);
            
            await reader.CompleteAsync();
            
            Disconnect();
        }
    }

    protected async ValueTask WriteDataAsync(byte[] packet)
    {
        await _sendPipe.Writer.WriteAsync(packet);
        await _sendPipe.Writer.FlushAsync();
    }

    protected abstract ValueTask ProcessPacketAsync(ReadOnlySequence<byte> packet);
    
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
        
        _logger.Verbose("Disposing connection {ConnectionId}", _id);
        
        _disposed = true;
        _sendPipe.Writer.CancelPendingFlush();
        _sendPipe.Reader.CancelPendingRead();
        _recvPipe.Writer.CancelPendingFlush();
        _recvPipe.Reader.CancelPendingRead();
        
        Disconnect();
    }
}