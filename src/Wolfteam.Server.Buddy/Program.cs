// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-02-05.

using Serilog;
using Serilog.Events;
using Wolfteam.Server;
using Wolfteam.Server.Buddy;
using Wolfteam.Server.Utils;

// Setup.
var cancelTokenSource = new CancellationTokenSource();
var cancelToken = cancelTokenSource.Token;

// Logging.
Log.Logger = SerilogConfig.CreateDefault()
    .CreateLogger();

Console.CancelKeyPress += (_, args) =>
{
    args.Cancel = true;
    cancelTokenSource.Cancel();
};

using var server = new BuddyServer(40950);

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