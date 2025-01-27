// Copyright (c) AeonLucid. All Rights Reserved.
// Licensed under the AGPL-3.0 License.
// Solution Wolfteam, Date 2025-01-27.

using Wolfteam.Packets;

namespace Wolfteam.Server.Tests;

public class PacketRegressionTests
{
    [Test]
    public void CS_FD_CREATE_REQ_Regression()
    {
        var data = Convert.FromHexString("305400650061006d0077006f0072006b00210020005100750069007400200073006c00610063006b0069006e0067002100003c020000000505580264000200c000105400000000000000003d64001900");
        var packet = PacketSerializer.TryDeserialize(PacketId.CS_FD_CREATE_REQ, ClientVersion.IS_854, data, out var wolfPacket);
        
        Assert.That(packet, Is.True);
    }
}