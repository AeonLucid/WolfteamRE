// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-29.

using Serilog;

namespace Wolfteam.Server.Broker;

public class BrokerServer : WolfServer<BrokerConnection>
{
    private static readonly ILogger Logger = Log.ForContext<BrokerServer>();
    
    public BrokerServer(int port) : base(Logger, port)
    {
    }
}