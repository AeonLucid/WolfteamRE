// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-21.

using Serilog;
using Wolfteam.Server;
using Wolfteam.Server.Login;
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

using var server = new LoginServer(8444);

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
