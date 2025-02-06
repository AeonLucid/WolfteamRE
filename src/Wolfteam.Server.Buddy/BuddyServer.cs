// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-02-05.

using Serilog;

namespace Wolfteam.Server.Buddy;

public class BuddyServer : WolfServer<BuddyConnection>
{
    private static readonly ILogger Logger = Log.ForContext<BuddyServer>();
    
    public BuddyServer(int port) : base(Logger, port)
    {
    }
}