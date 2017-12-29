using System;
using System.IO;
using Ionic.Zlib;
using Wolfteam.Tools.XfsEditor.Xfs.Exceptions;

namespace Wolfteam.Tools.XfsEditor.Xfs
{
    public class XfsFile
    {
        public XfsFile(string fileName)
        {
            FileName = fileName;
        }

        public string FileName { get; }

        public void Read()
        {
            using (var fileStream = File.Open(FileName, FileMode.Open))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                // Read file header.
                var fileSize = fileStream.Length;
                var offset = binaryReader.ReadUInt32();
                
                if (fileSize > 0xFFFFFFFF)
                {
                    var offset48 = binaryReader.ReadUInt16();

                    offset |= (uint)(offset48 << 32);
                }

                binaryReader.BaseStream.Position = offset;
                
                // Read xfs header.
                var header = new XfsHeader();
                var zSize = binaryReader.ReadByte();
                var headerData = binaryReader.ReadBytes(zSize);
                var headerDataDecompressed = ZlibStream.UncompressBuffer(headerData);

                using (var headerReader = new BinaryReader(new MemoryStream(headerDataDecompressed)))
                {
                    header.Junk = headerReader.ReadBytes(3);
                    header.Version = headerReader.ReadByte();
                    header.Dummy = headerReader.ReadUInt32();
                    header.Entries = headerReader.ReadUInt32();
                    header.Zero = headerReader.ReadUInt32();
                    header.XfsOffset = headerReader.ReadUInt32();
                }

                // Read xfs sizes.
                uint infoSize;
                uint infoZSize;

                if ((char)header.Version == ' ')
                {
                    var infoSizeBytes = binaryReader.ReadBytes(3);
                    var infoZSizeBytes = binaryReader.ReadBytes(3);

                    infoSize = BitConverter.ToUInt32(new byte[] { infoSizeBytes[0], infoSizeBytes[1], infoSizeBytes[2], 0x00 }, 0);
                    infoZSize = BitConverter.ToUInt32(new byte[] { infoZSizeBytes[0], infoZSizeBytes[1], infoZSizeBytes[2], 0x00 }, 0);
                }
                else
                {
                    var infoZSizeBytes = binaryReader.ReadBytes(3);

                    infoSize = header.Entries * 0x80;
                    infoZSize = BitConverter.ToUInt32(new byte[] { infoZSizeBytes[0], infoZSizeBytes[1], infoZSizeBytes[2], 0x00 }, 0);
                }

                // Read xfs body.
                var bodyData = binaryReader.ReadBytes((int) infoZSize);
                var bodyDataUncompressed = ZlibStream.UncompressBuffer(bodyData);

                if (header.Version == ' ')
                {
                    // Do stuff.
                }
                else if (header.Version == '2')
                {
                    // Do stuff.
                }
                else
                {
                    throw new XfsVersionUnsupportedException($"Xfs version 0x{header.Version:X} is unsupported.");
                }
            }
        }
    }
}
