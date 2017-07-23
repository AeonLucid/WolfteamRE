using System;
using System.IO;
using Wolfteam.Playzone.Gunbound;
using Wolfteam.Playzone.Util;

namespace Wolfteam.Playzone
{
    /// <summary>
    ///     This is just a small app to figure out some variables and verify stuff.
    /// </summary>
    internal class Program
    {
        private static int _packetCount;

        private static void Main()
        {
            Console.Title = "Wolfteam.Playzone";
            Crypto.Initialize();

            // Place DoStuff(aeskey (hex), initial_login_packet (hex)) here.

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static void DoStuff(string aesKey, string packetHex)
        {
            Console.WriteLine($"=========== Login packet inspection #{_packetCount++} ======================================");

            var packet = HexUtil.StringToByteArray(packetHex);
            var firstPacket = new PacketReader(packet, packet.Length);
            
            var decryptme1 = firstPacket.ReadBytes(16);
            var decryptme2 = firstPacket.ReadBytes(16);

            var username = Utils.GetASCIIZ(Crypto.DecryptStaticBuffer(decryptme1));

            Console.WriteLine($"{"Username:",-20}\"{username}\"");

            var secondPacket = new PacketReader(Crypto.DecryptStaticBuffer(decryptme2), 16);
            secondPacket.Seek(0, SeekOrigin.Begin);

            Console.WriteLine("SecondPacket:");
            Console.WriteLine(HexUtil.HexDump(secondPacket.Buffer));

            var authDword = secondPacket.ReadUInt32();
            var unknown = secondPacket.ReadBytes(12);

            Console.WriteLine($"{"AuthRandom:",-20}\"{authDword}\"");
            Console.WriteLine("Unknown:");
            Console.WriteLine(HexUtil.HexDump(unknown));

            var secondPacketCrypto = new Crypto(aesKey);

            var input = firstPacket.ReadBytes(32);
            var output = new byte[24];

            Console.WriteLine("Decrypt input:");
            Console.WriteLine(HexUtil.HexDump(input));

            if (!secondPacketCrypto.PacketDecrypt(input, ref output, 4114))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Login decryption failed.");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("Decrypt output:");
                Console.WriteLine(HexUtil.HexDump(output));

                var packetReader2 = new PacketReader(output, output.Length);
                packetReader2.Seek(0, SeekOrigin.Begin);

                var password = Utils.GetASCIIZ(packetReader2.ReadBytes(20));

                Console.WriteLine($"{"Password:",-20}\"{password}\"");

                var clientVer = packetReader2.ReadUInt32();
                Console.WriteLine($"{"ClientVer:",-20}\"{clientVer}\"");
            }
        }
    }
}
