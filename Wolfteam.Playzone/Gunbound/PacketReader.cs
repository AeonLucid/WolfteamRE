using System;
using System.IO;
using System.Text;

namespace Wolfteam.Playzone.Gunbound
{
    /// <summary>
    ///     File from https://github.com/CarlosX/GunBoundWC.
    /// </summary>
    public class PacketReader
    {
        private byte[] m_Buffer;
        private int m_Length;
        private int m_Index;

        public byte[] Buffer
        {
            get
            {
                return this.m_Buffer;
            }
        }

        public int Length
        {
            get
            {
                return this.m_Length;
            }
        }

        public PacketReader(byte[] buffer, int length)
        {
            this.m_Buffer = buffer;
            this.m_Length = length;
            this.m_Index = 6;
        }

        public void Seek(int offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    this.m_Index = offset;
                    break;
                case SeekOrigin.Current:
                    this.m_Index += offset;
                    break;
                case SeekOrigin.End:
                    this.m_Index = this.m_Length - offset;
                    break;
            }
        }

        public int ReadInt32()
        {
            if (this.m_Index + 4 > this.m_Length)
                return 0;
            return (int)this.m_Buffer[this.m_Index++] | (int)this.m_Buffer[this.m_Index++] << 8 | (int)this.m_Buffer[this.m_Index++] << 16 | (int)this.m_Buffer[this.m_Index++] << 24;
        }

        public short ReadInt16()
        {
            if (this.m_Index + 2 > this.m_Length)
                return 0;
            return (short)((int)this.m_Buffer[this.m_Index++] | (int)this.m_Buffer[this.m_Index++] << 8);
        }

        public byte ReadByte()
        {
            if (this.m_Index + 1 > this.m_Length)
                return 0;
            return this.m_Buffer[this.m_Index++];
        }

        public uint ReadUInt32()
        {
            if (this.m_Index + 4 > this.m_Length)
                return 0;
            return (uint)((int)this.m_Buffer[this.m_Index++] | (int)this.m_Buffer[this.m_Index++] << 8 | (int)this.m_Buffer[this.m_Index++] << 16 | (int)this.m_Buffer[this.m_Index++] << 24);
        }

        public ushort ReadUInt16()
        {
            if (this.m_Index + 2 > this.m_Length)
                return 0;
            return (ushort)((int)this.m_Buffer[this.m_Index++] | (int)this.m_Buffer[this.m_Index++] << 8);
        }

        public sbyte ReadSByte()
        {
            if (this.m_Index + 1 > this.m_Length)
                return 0;
            return (sbyte)this.m_Buffer[this.m_Index++];
        }

        public bool ReadBoolean()
        {
            if (this.m_Index + 1 > this.m_Length)
                return false;
            return (int)this.m_Buffer[this.m_Index++] != 0;
        }

        public string ReadString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            while (true)
            {
                byte num1 = 0;
                int num2;
                if (this.m_Index < this.m_Length)
                {
                    byte[] buffer = this.m_Buffer;
                    int index = this.m_Index++;
                    num2 = (int)(num1 = buffer[index]) != 0 ? 1 : 0;
                }
                else
                    num2 = 0;
                if (num2 != 0)
                    stringBuilder.Append((char)num1);
                else
                    break;
            }
            return stringBuilder.ToString();
        }

        public string ReadString(int size)
        {
            int num1 = this.m_Index + size;
            int num2 = num1;
            if (num1 > this.m_Length)
                num1 = this.m_Length;
            StringBuilder stringBuilder = new StringBuilder();
            while (true)
            {
                byte num3 = 0;
                int num4;
                if (this.m_Index < num1)
                {
                    byte[] buffer = this.m_Buffer;
                    int index = this.m_Index++;
                    num4 = (int)(num3 = buffer[index]) != 0 ? 1 : 0;
                }
                else
                    num4 = 0;
                if (num4 != 0)
                    stringBuilder.Append((char)num3);
                else
                    break;
            }
            this.m_Index = num2;
            return stringBuilder.ToString();
        }

        public byte[] ReadBytes(int size)
        {
            int num1 = this.m_Index + size;
            int num2 = num1;
            if (num1 > this.m_Length)
                num1 = this.m_Length;
            byte[] numArray = new byte[size];
            System.Buffer.BlockCopy((Array)this.m_Buffer, this.m_Index, (Array)numArray, 0, num1 - this.m_Index);
            this.m_Index = num2;
            return numArray;
        }

//        public void Trace(NetState state, int packetID)
//        {
//            try
//            {
//                using (StreamWriter streamWriter = new StreamWriter("Logs\\packets.log", true))
//                {
//                    byte[] buffer = this.m_Buffer;
//                    if (buffer.Length < 0)
//                        streamWriter.WriteLine("Client {0} : Unhandled packet 0x{1:X2}", (object)state, (object)packetID);
//                    using (MemoryStream memoryStream = new MemoryStream(buffer))
//                        Utils.FormatBuffer((TextWriter)streamWriter, (Stream)memoryStream, buffer.Length);
//                    streamWriter.WriteLine();
//                    streamWriter.WriteLine();
//                }
//            }
//            catch
//            {
//            }
//        }
    }
}
