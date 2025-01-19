using System.Net;
using System.Net.Sockets;
using Wolfteam.Server.Login;

var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

server.Bind(new IPEndPoint(IPAddress.Any, 8444));
server.Listen(10);

Console.WriteLine("Listening");

void ParseLoginPacket(ReadOnlySpan<byte> data)
{
    var reader = new SpanReader(data);
    
    var packetLen = reader.ReadU32();
    var packetId = reader.ReadU16();
    
    Console.WriteLine($"  packetLen {packetLen}");
    Console.WriteLine($"  packetId {packetId}");

    // Encrypted by static key
    var subPacketOne = new SpanReader(reader.ReadBytes(32));
    
    Console.WriteLine(Convert.ToHexString(subPacketOne.ReadBytes(32)));
    
    var subPacketTwo = new SpanReader(reader.ReadBytes(32));
    
    Console.WriteLine(Convert.ToHexString(subPacketTwo.ReadBytes(32)));
}

while (true)
{
    var client = await server.AcceptAsync();
    Console.WriteLine("Accepted connection from " + client.RemoteEndPoint);
    
    var buffer = new byte[1024];
    var received = await client.ReceiveAsync(buffer);
    
    Console.WriteLine("Received " + received + " bytes");
    Console.WriteLine(Convert.ToHexString(buffer.AsSpan(0, received)));
    
    ParseLoginPacket(buffer.AsSpan(0, received));
    
    await client.SendAsync("Hello from server!"u8.ToArray());
    client.Close();
}