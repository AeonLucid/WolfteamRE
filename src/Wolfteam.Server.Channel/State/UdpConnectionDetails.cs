// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-27.

using System.Net;

namespace Wolfteam.Server.Channel.State;

public class UdpConnectionDetails
{
    public IPEndPoint? RemoteEndPoint { get; set; }
    
    public IPEndPoint? LocalEndPoint { get; set; }
}