using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wolfteam.Client.Net.Crypt;

namespace Wolfteam.Client.Net.Wolfteam
{
    /// <summary>
    ///     Composes a packet.
    ///     Packet format:
    ///         - Length    (int, 4 bytes)
    ///         - Id        (short, 2 bytes)
    ///         - Payload   (?????, length - 6)
    /// </summary>
    internal class PacketComposer : IDisposable
    {
        public PacketComposer(PacketOut packet)
        {
            Id = packet;
            Memory = new MemoryStream();
            Writer = new BinaryWriter(Memory);
            Payloads = new List<byte[]>();
        }

        public PacketOut Id { get; }

        public MemoryStream Memory { get; }

        public BinaryWriter Writer { get; }

        public List<byte[]> Payloads { get; }

        /// <summary>
        ///     This will be added to the packet without interfering with the normal payload.
        ///     TODO: Figure out the proper way Wolfteam does this.
        /// </summary>
        /// <param name="payload"></param>
        public void AppendPayload(byte[] payload)
        {
            Payloads.Add(payload);
        }

        public async Task<byte[]> ToArrayAsync()
        {
            // Construct final packet.
            using (var finalMemory = new MemoryStream())
            using (var finalWriter = new BinaryWriter(finalMemory))
            {
                // Append header.
                finalWriter.Write((int)Memory.Length + 6 + Payloads.Sum(x => x.Length));
                finalWriter.Write((short)Id);

                // Append AES encrypted payload.
                finalWriter.Write(await AesCrypto.StaticEncryptAsync(Memory.ToArray()));

                // Append other payloads.
                foreach (var payload in Payloads)
                {
                    finalWriter.Write(payload);
                }

                return finalMemory.ToArray();
            }
        }

        public void Dispose()
        {
            Memory?.Dispose();
            Writer?.Dispose();
        }
    }
}
