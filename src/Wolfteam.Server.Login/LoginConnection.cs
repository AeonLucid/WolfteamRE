using System.Buffers;
using System.Net.Sockets;
using System.Security.Cryptography;
using Serilog;
using Wolfteam.Server.Login.Packets;
using static Wolfteam.Server.Login.Constants;

namespace Wolfteam.Server.Login;

public class LoginConnection : WolfConnection
{
    private static readonly ILogger Logger = Log.ForContext<LoginConnection>();
    
    public LoginConnection(Guid id, Socket client) : base(Logger, id, client)
    {
    }

    protected override async ValueTask ProcessPacketAsync(ReadOnlySequence<byte> packet)
    {
        Logger.Debug("Received packet {Packet}", Convert.ToHexStringLower(packet.ToArray()));

        var reader = new PacketReader(packet);

        var packetLen = reader.ReadInt();
        var packetId = reader.ReadShort();

        Logger.Debug("Packet {PacketId} {PacketLen}", packetId, packetLen);
        
        // Very simple packet handler, will implement a better one once more packets added.
        switch (packetId)
        {
            case 0x1310:
            {
                using var _cryptoStatic = Aes.Create();
                using var _cryptoAuth = Aes.Create();
                
                _cryptoStatic.Key = StaticKey;
                
                var staticData = reader.ReadBytes(32);
                var authData = reader.ReadBytes(32);
                
                if (!LoginRequest.TryParseStatic(_cryptoStatic, ref staticData, out var username, out var magic))
                {
                    Logger.Warning("Invalid username data");
                    return;
                }

                Logger.Information("  Username {Username}", username);
                Logger.Information("  Magic    {Magic}", magic);

                // TODO: Retrieve password hash from database.
                var passwordHash = MD5.HashData("qqq"u8.ToArray());
                
                _cryptoAuth.Key = LoginRequest.CreateAuthKey(username, passwordHash, magic);
                
                if (!LoginRequest.TryDecryptAuth(_cryptoAuth, ref authData, out var password, out var version))
                {
                    Logger.Warning("Invalid password data");
                    return;
                }
                
                Logger.Information("  Password {Password}", password);
                Logger.Information("  Version  {Version}", version);

                // 0x1312
                // - 0x00 = ? (+8 DWORD, +12 DWORD) or (+8 CHAR, + 24 CHAR)
                // - 0x03 = Can't connect due to too many users
                // - 0x10 = ?
                // - 0x11 = ?
                // - 0x19 = Uncertified ID
                // - 0x30 = Uncertified ID (But also accepts more data)
                // - 0x60 = Incorrect client version
                // - 0x91 = Uncertified ID
                // - 0x95 = Can't connect due to too many users
                
                await WriteDataAsync([
                    0x08, 0x00, 0x00, 0x00,
                    0x12, 0x13, 0x00, 0x00
                ]);
                break;
            }
            case 0x1320:
            {
                Logger.Warning("Unhandled packet {PacketId}, FacebookLogin", packetId);
                break;
            }
            case 0x4000:
            {
                Logger.Warning("Unhandled packet {PacketId}, Pc data?", packetId);
                
                // 0x4001
                // - 0x30 = Game play prohibited
                // await WriteDataAsync([
                //     0x2C, 0x00, 0x00, 0x00,
                //     0x01, 0x40, 0x30, 0x00,
                //     0x00, 0x00, 0x00, 0x00, // tm_sec
                //     0x10, 0x00, 0x00, 0x00, // tm_min
                //     0x00, 0x00, 0x00, 0x00, // tm_hour
                //     0x00, 0x00, 0x00, 0x00, // tm_mday
                //     0x00, 0x00, 0x00, 0x00, // tm_mon
                //     0x00, 0x00, 0x00, 0x00, // tm_year
                //     0x00, 0x00, 0x00, 0x00, // tm_wday
                //     0x00, 0x00, 0x00, 0x00, // tm_yday
                //     0x00, 0x00, 0x00, 0x00, // tm_isdst
                // ]);
                
                // Start game
                await WriteDataAsync([
                    0x08, 0x00, 0x00, 0x00,
                    0x01, 0x40, 0x01, 0x90,
                ]);
                
                // Specify third launch parameter.
                // await WriteDataAsync([
                //     0x0C, 0x00, 0x00, 0x00,
                //     0x01, 0x40, 0x01, 0x90,
                //     0x05, 0x00, 0x00, 0x00,
                // ]);
                break;
            }
            default:
            {
                Logger.Warning("Unhandled packet {PacketId}", packetId);
                break;
            }
        }
        
        // if (!reader.Completed)
        // {
        //     Logger.Warning("Packet {PacketId} has {Bytes} bytes remaining", packetId, reader.Remaining);
        // }
    }
}