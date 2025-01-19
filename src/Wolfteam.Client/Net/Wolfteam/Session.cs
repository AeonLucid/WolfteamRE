using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Wolfteam.Client.Config;
using Wolfteam.Client.Net.Crypt;
using Wolfteam.Client.Util;

namespace Wolfteam.Client.Net.Wolfteam
{
    internal class Session : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly AccountConfig _account;

        private readonly AesCrypto _crypto;

        private readonly TcpClient _client;

        public Session(AccountConfig account)
        {
            if (account.Username.Length > 16)
            {
                throw new ArgumentException("Account username must be 16 characters or less.");
            }

            if (account.Password.Length > 20)
            {
                throw new ArgumentException("Account password must be 20 characters or less.");
            }

            _account = account;
            _crypto = new AesCrypto(HexUtil.HexToBytes(_account.AesKey));
            _client = new TcpClient();
        }

        /// <summary>
        ///     Gets the username of the <see cref="_account"/>.
        /// </summary>
        public string Username => _account.Username;

        /// <summary>
        ///     Gets the connected status of this <see cref="Session"/>.
        /// </summary>
        public bool Connected => _client.Connected;

        /// <summary>
        ///     Gets the listening state of this <see cref="Session"/>.
        /// </summary>
        public bool Listening { get; private set; }

        private CancellationTokenSource ListenerCancellation { get; set; }

        /// <summary>
        ///     Connects this <see cref="Session"/> to Wolfteam.
        /// </summary>
        /// <returns>Returns <code>true</code> if the <see cref="Session"/> successfully connected to Wolfteam.</returns>
        public async Task<bool> ConnectAsync()
        {
            await _client.ConnectAsync(WolfteamConfig.LoginIp, WolfteamConfig.LoginPort);

            return _client.Connected;
        }

        /// <summary>
        ///     Starts a new <see cref="Thread"/> to receive data back from Wolfteam.
        /// </summary>
        public void StartListening()
        {
            if (Listening)
            {
                throw new Exception("Session is already listening.");
            }

            Listening = true;
            ListenerCancellation = new CancellationTokenSource();

            new Thread(Listener)
            {
                IsBackground = true     // Make sure we don't have a deadlock when trying to close this application.
            }.Start();
        }

        /// <summary>
        ///     Stop the listening <see cref="Thread"/>.
        /// </summary>
        public void StopListening()
        {
            Listening = false;
            ListenerCancellation.Cancel();
        }

        /// <summary>
        ///     Simple receiver because we are a bit lazy.
        /// </summary>
        private async void Listener(object obj)
        {
            var buffer = new byte[2048];
            
            while (Listening)
            {
                var byteCount = await _client.GetStream().ReadAsync(buffer, 0, buffer.Length, ListenerCancellation.Token);
                if (byteCount == 0)
                {
                    Logger.Error("Wolfteam closed the connection.");

                    _client.Close();
                    return;
                }

                var packet = new byte[byteCount];
                Buffer.BlockCopy(buffer, 0, packet, 0, byteCount);

                Logger.Info("Received data from Wolfteam.");
                Logger.Info(BitConverter.ToString(packet));
            }
        }

        /// <summary>
        ///     Constructs and sends the <see cref="PacketOut.Login"/> packet to Wolfteam.
        /// </summary>
        /// <returns></returns>
        public async Task LoginAsync()
        {
            using (var packet = new PacketComposer(PacketOut.Login))
            {
                // Add username (16 bytes).
                packet.Writer.Write(Encoding.ASCII.GetBytes(_account.Username));    // Username.
                packet.Writer.Write(new byte[16 - _account.Username.Length]);       // Username padding (to 16 bytes).

                // Add unknown  (16 bytes).
                packet.Writer.Write(_account.AuthRandom);               // Auth uk (4 bytes).
                packet.Writer.Write(HexUtil.HexToBytes("60F41900"));    // Unknown (4 bytes).
                packet.Writer.Write(HexUtil.HexToBytes("41F3DADA"));    // Unknown (4 bytes) (Changes in between requests through real client).
                packet.Writer.Write(HexUtil.HexToBytes("60F41900"));    // Unknown (4 bytes).

                // Add second payload (32 bytes)
                using (var subPacketStream = new MemoryStream(32))
                using (var subPacket = new BinaryWriter(subPacketStream))
                {
                    subPacket.Write(Encoding.ASCII.GetBytes(_account.Password));    // Password.
                    subPacket.Write(new byte[20 - _account.Password.Length]);       // Password padding (to 20 bytes).
                    subPacket.Write(WolfteamConfig.ClientVersion);                  // Client version (4 bytes).
                    
                    // Add checksums.
                    const int subPacketId = 4114;
                    var subPacketBuffer = subPacketStream.ToArray();
                    if (subPacketBuffer.Length % 12 != 0)
                    {
                        throw new Exception("Buffer length must be a multiple of 12.");
                    }

                    // Write checksums.
                    subPacket.Seek(0, SeekOrigin.Begin);
                    
                    for (var index = 0; index < subPacketBuffer.Length / 12; index++)
                    {
                        subPacket.Write(WolfteamConfig.MagicChecksum + subPacketId);
                        subPacket.Write(subPacketBuffer, index * 12, 12);
                    }

                    // Append to main packet.
                    packet.AppendPayload(await _crypto.EncryptAsync(subPacketStream.ToArray()));
                }

                await SendPacketAsync(packet);
            }
        }

        /// <summary>
        ///     Sends a <see cref="PacketComposer"/> to Wolfteam.
        /// </summary>
        /// <param name="packet">The packet you want to send.</param>
        private async Task SendPacketAsync(PacketComposer packet)
        {
            if (!Connected)
            {
                throw new Exception("Session is not connected to Wolfteam.");
            }

            if (!Listening)
            {
                Logger.Warn("You are sending a packet without listening for a response.");
            }

            var buffer = await packet.ToArrayAsync();

            await _client.GetStream().WriteAsync(buffer, 0, buffer.Length);
        }

        public void Dispose()
        {
            _crypto?.Dispose();
            _client?.Dispose();
            ListenerCancellation?.Cancel(false);
            ListenerCancellation?.Dispose();
        }
    }
}
