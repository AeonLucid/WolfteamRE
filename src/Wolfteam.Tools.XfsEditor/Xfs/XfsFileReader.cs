using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ionic.Zlib;
using Wolfteam.Tools.XfsEditor.Extensions;
using Wolfteam.Tools.XfsEditor.Xfs.Exceptions;

namespace Wolfteam.Tools.XfsEditor.Xfs
{
    public class XfsFileReader : IDisposable
    {
        private FileStream _fileStream;

        private BinaryReader _binaryReader;

        public XfsFileReader(string fileName)
        {
            FileName = fileName;
            Header = null;
            Files = new List<XfsFileRead>();
        }

        public string FileName { get; }

        public XfsHeader Header { get; set; }

        public List<XfsFileRead> Files { get; set; }

        public void Run()
        {
            // Initialize streams.
            _fileStream = File.Open(FileName, FileMode.Open);
            _binaryReader = new BinaryReader(_fileStream);

            // Read file header.
            var fileSize = _fileStream.Length;
            var offset = _binaryReader.ReadUInt32();

            if (fileSize > 0xFFFFFFFF)
            {
                var offset48 = _binaryReader.ReadUInt16();

                offset |= (uint) (offset48 << 32);
            }

            _binaryReader.BaseStream.Position = offset;

            // Read xfs header.
            var header = new XfsHeader();
            var zSize = _binaryReader.ReadByte();
            var headerData = _binaryReader.ReadBytes(zSize);
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

            Header = header;

            // Read xfs sizes.
            uint infoSize;
            uint infoZSize;

            if ((char)Header.Version == ' ')
            {
                infoSize = _binaryReader.ReadThreeByte();
                infoZSize = _binaryReader.ReadThreeByte();
            }
            else
            {
                infoSize = Header.Entries * 0x80;
                infoZSize = _binaryReader.ReadThreeByte();
            }

            // Read xfs body.
            var bodyData = _binaryReader.ReadBytes((int) infoZSize);
            var bodyDataUncompressed = ZlibStream.UncompressBuffer(bodyData);

            if (Header.Version == ' ')
            {
                using (var memoryStream = new MemoryStream(bodyDataUncompressed))
                using (var bodyReader = new BinaryReader(memoryStream))
                {
                    ExtractFileData(bodyReader);
                }
            }
            else if (Header.Version == '2')
            {
                // Do stuff.
            }
            else
            {
                throw new XfsVersionUnsupportedException($"Xfs version 0x{header.Version:X} is unsupported.");
            }
        }

        private void ExtractFileData(BinaryReader bodyReader, string path = "", string name = "", XfsFileRead parent = null)
        {
            path += $"{name}/";

            var entries = bodyReader.ReadUInt32();
            var namesz = bodyReader.ReadByte();
            name = Encoding.ASCII.GetString(bodyReader.ReadBytes(namesz));
            var offset = bodyReader.ReadUInt32();
            var offset48 = bodyReader.ReadUInt16();
            offset |= (uint) (offset48 << 32);
            var size = bodyReader.ReadUInt32();
            var sizeCompressed = bodyReader.ReadUInt32();
            var isFile = Convert.ToBoolean(bodyReader.ReadByte());

            if (isFile)
            {
                if (parent == null)
                {
                    throw new XfsException("File does not have a parent.");
                }

                parent.Children.Add(new XfsFileRead(name, offset, size, sizeCompressed, true));
            }
            else
            {
                var directory = new XfsFileRead(name, offset, size, sizeCompressed, false);

                for (var i = 0; i < entries; i++)
                {
                    ExtractFileData(bodyReader, path, name, directory);
                }
                
                if (parent != null)
                {
                    parent.Children.Add(directory);

                    directory.Parent = parent;
                }
                else
                {
                    Files.Add(directory);
                }
            }
        }

        private void ExtractFile(BinaryReader reader, string name, uint offset, uint zsize)
        {
            var endOffset = offset + zsize;
            var fileBytes = new List<byte>();

            while (reader.BaseStream.Position != endOffset)
            {
                var chunkSize = reader.ReadThreeByte();
                var flags = chunkSize >> 22;

                uint chunkzSize = 0;

                if ((flags & 2) != 0)
                {
                    if ((flags & 1) != 0)
                    {
                    }
                    else
                    {
                        chunkzSize = _binaryReader.ReadThreeByte();
                    }
                }

                if (flags == 0)
                {
                    chunkzSize = chunkSize;
                    chunkSize = 0x10000;
                }

                var dummy = reader.ReadInt16();

                if ((flags & 1) != 0)
                {
                    fileBytes.AddRange(reader.ReadBytes((int) chunkSize));
                }
                else
                {
                    if (chunkzSize == 0)
                    {
                        throw new Exception("uh");
                    }

                    var bytesCompressed = reader.ReadBytes((int) chunkzSize);
                    fileBytes.AddRange(ZlibStream.UncompressBuffer(bytesCompressed));
                }
            }
        }

        public void Dispose()
        {
            _binaryReader?.Dispose();
            _fileStream?.Dispose();
        }
    }
}