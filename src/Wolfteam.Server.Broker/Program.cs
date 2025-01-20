using Serilog;
using Wolfteam.Server;
using Wolfteam.Server.Broker;
using Wolfteam.Server.Utils;

// Setup.
var cancelTokenSource = new CancellationTokenSource();
var cancelToken = cancelTokenSource.Token;

// Logging.
Log.Logger = SerilogConfig.CreateDefault().CreateLogger();

Console.CancelKeyPress += (_, args) =>
{
    args.Cancel = true;
    cancelTokenSource.Cancel();
};

using var server = new WolfServer<BrokerConnection>(40706);

server.Listen();

try
{
    while (!cancelToken.IsCancellationRequested)
    {
        await server.AcceptAsync(cancelToken);
    }
}
catch (OperationCanceledException)
{
    Log.Information("Server stopped");
}