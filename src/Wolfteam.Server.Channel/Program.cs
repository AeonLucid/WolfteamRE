﻿// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-21.

using Serilog;
using Serilog.Events;
using Wolfteam.Server;
using Wolfteam.Server.Channel;
using Wolfteam.Server.Utils;

// Setup.
var cancelTokenSource = new CancellationTokenSource();
var cancelToken = cancelTokenSource.Token;

// Logging.
Log.Logger = SerilogConfig.CreateDefault()
    .MinimumLevel.Override(typeof(ChannelConnection).FullName!, LogEventLevel.Debug)
    .CreateLogger();

Console.CancelKeyPress += (_, args) =>
{
    args.Cancel = true;
    cancelTokenSource.Cancel();
};

using var server = new WolfServer<ChannelConnection>(40850);

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