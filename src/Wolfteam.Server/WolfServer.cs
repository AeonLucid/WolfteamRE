// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-21.

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Serilog;

namespace Wolfteam.Server;

public class WolfServer<T> : IDisposable where T : WolfConnection
{
    private readonly ILogger _logger;
    private readonly int _port;
    private readonly Socket _socket;

    private bool _disposed;
    
    public WolfServer(ILogger logger, int port)
    {
        _logger = logger;
        _port = port;
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
        Connections = new ConcurrentDictionary<Guid, WolfConnection>();
    }
    
    protected ConcurrentDictionary<Guid, WolfConnection> Connections { get; }

    public virtual void Listen()
    {
        _socket.Bind(new IPEndPoint(IPAddress.Any, _port));
        _socket.Listen(10);
        
        _logger.Information("Listening on port {Port}", _port);
    }

    public async Task AcceptAsync(CancellationToken cancellationToken)
    {
        var client = await _socket.AcceptAsync(cancellationToken);
        var clientId = Guid.NewGuid();
        var connection = OnConnectionAccepted(clientId, client);

        if (!Connections.TryAdd(clientId, connection))
        {
            throw new InvalidOperationException("Failed to add connection to connections dictionary");
        }
        
        _logger.Information("Accepted connection {ConnectionId}", clientId);
        
        connection.ConnectionClosed += OnConnectionClosed;
        connection.Start();
    }
    
    protected virtual T OnConnectionAccepted(Guid clientId, Socket socket)
    {
        return (T) Activator.CreateInstance(typeof(T), clientId, socket)!;
    }

    protected virtual Task OnConnectionClosed(WolfConnection connection)
    {
        return Task.CompletedTask;
    }

    public virtual void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        
        _disposed = true;
        _socket.Dispose();

        foreach (var (_, value) in Connections)
        {
            value.Dispose();
        }
    }
}