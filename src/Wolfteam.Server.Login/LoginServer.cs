// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-29.

using Serilog;

namespace Wolfteam.Server.Login;

public class LoginServer : WolfServer<LoginConnection>
{
    private static readonly ILogger Logger = Log.ForContext<LoginServer>();
    
    public LoginServer(int port) : base(Logger, port)
    {
    }
}