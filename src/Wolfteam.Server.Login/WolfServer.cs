using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Serilog;

namespace Wolfteam.Server.Login;

public class WolfServer : IDisposable
{
    private static readonly ILogger Logger = Log.ForContext<WolfServer>();
    
    private readonly int _port;
    private readonly Socket _socket;
    private readonly ConcurrentDictionary<Guid, WolfConnection> _connections;

    private bool _disposed;
    
    public WolfServer(int port)
    {
        _port = port;
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _connections = new ConcurrentDictionary<Guid, WolfConnection>();
    }

    public void Listen()
    {
        _socket.Bind(new IPEndPoint(IPAddress.Any, _port));
        _socket.Listen(10);
        
        Logger.Information("Listening on port {Port}", _port);
    }

    public async Task AcceptAsync(CancellationToken cancellationToken)
    {
        var client = await _socket.AcceptAsync(cancellationToken);
        var clientId = Guid.NewGuid();
        var connection = new WolfConnection(clientId, client);

        if (!_connections.TryAdd(clientId, connection))
        {
            throw new InvalidOperationException("Failed to add connection to connections dictionary");
        }
        
        Logger.Information("Accepted connection {ConnectionId}", clientId);

        connection.Start();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        
        _disposed = true;
        _socket.Dispose();

        foreach (var (_, value) in _connections)
        {
            value.Dispose();
        }
    }
}