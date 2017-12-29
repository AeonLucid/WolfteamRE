using System;
using System.IO;

namespace Wolfteam.Tools.XfsEditor.Extensions
{
    public static class BinaryReaderExtensions
    {
        public static uint ReadThreeByte(this BinaryReader reader)
        {
            var bytes = reader.ReadBytes(3);

            return BitConverter.ToUInt32(new byte[] { bytes[0], bytes[1], bytes[2], 0x00 }, 0);
        }
    }
}
