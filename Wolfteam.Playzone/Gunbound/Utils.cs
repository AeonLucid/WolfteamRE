using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Wolfteam.Playzone.Gunbound
{
    /// <summary>
    ///     File from https://github.com/CarlosX/GunBoundWC.
    /// </summary>
    public class Utils
    {
        private static Encoding m_Encoding = Encoding.GetEncoding("Latin1");
        private static Random m_Random = new Random((int)((DateTime.Now.ToFileTime() ^ (long)(int)Utils.GetCurrentThreadId()) % (long)int.MaxValue));

        public static Encoding Encoding
        {
            get
            {
                return Utils.m_Encoding;
            }
        }

        public static void FormatBuffer(TextWriter output, Stream input, int length)
        {
            output.WriteLine("        0  1  2  3  4  5  6  7   8  9  A  B  C  D  E  F");
            output.WriteLine("       -- -- -- -- -- -- -- --  -- -- -- -- -- -- -- --");
            int num1 = 0;
            int num2 = length >> 4;
            int capacity = length & 15;
            int num3 = 0;
            while (num3 < num2)
            {
                StringBuilder stringBuilder1 = new StringBuilder(49);
                StringBuilder stringBuilder2 = new StringBuilder(16);
                for (int index = 0; index < 16; ++index)
                {
                    int num4 = input.ReadByte();
                    stringBuilder1.Append(num4.ToString("X2"));
                    if (index != 7)
                        stringBuilder1.Append(' ');
                    else
                        stringBuilder1.Append("  ");
                    if (num4 >= 32 && num4 < 128)
                        stringBuilder2.Append((char)num4);
                    else
                        stringBuilder2.Append('.');
                }
                output.Write(num1.ToString("X4"));
                output.Write("   ");
                output.Write(stringBuilder1.ToString());
                output.Write("  ");
                output.WriteLine(stringBuilder2.ToString());
                ++num3;
                num1 += 16;
            }
            if (capacity == 0)
                return;
            StringBuilder stringBuilder3 = new StringBuilder(49);
            StringBuilder stringBuilder4 = new StringBuilder(capacity);
            for (int index = 0; index < 16; ++index)
            {
                if (index < capacity)
                {
                    int num4 = input.ReadByte();
                    stringBuilder3.Append(num4.ToString("X2"));
                    if (index != 7)
                        stringBuilder3.Append(' ');
                    else
                        stringBuilder3.Append("  ");
                    if (num4 >= 32 && num4 < 128)
                        stringBuilder4.Append((char)num4);
                    else
                        stringBuilder4.Append('.');
                }
                else
                    stringBuilder3.Append("   ");
            }
            output.Write(num1.ToString("X4"));
            output.Write("   ");
            output.Write(stringBuilder3.ToString());
            output.Write("  ");
            output.WriteLine(stringBuilder4.ToString());
        }

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        private static void LogInternal(string s)
        {
            Console.WriteLine(s);
        }

        public static void Log(string s)
        {
            Utils.LogInternal(string.Format("({0:MM}/{0:dd}/{0:yyyy} {0:T}) {1}", (object)DateTime.Now, (object)s));
        }

        public static void Log(Exception e)
        {
            Utils.Log(e.ToString());
        }

        public static void Log(string format, params object[] args)
        {
            Utils.Log(string.Format(format, args));
        }

        public static unsafe string GetASCIIZ(byte[] buffer)
        {
            string str;
            fixed (byte* numPtr = buffer)
                str = new string((sbyte*)numPtr);
            return str;
        }

        public static byte[] GetBytes(string s)
        {
            byte[] numArray = new byte[s.Length];
            int num = 0;
            foreach (char ch in s)
                numArray[num++] = (byte)ch;
            return numArray;
        }

        public static unsafe bool CompareBuffers(byte[] buffer1, byte[] buffer2)
        {
            int num = 0;
            int length;
            if ((length = buffer1.Length) != buffer2.Length)
                return false;
            fixed (byte* numPtr1 = buffer1)
            fixed (byte* numPtr2 = buffer2)
            {
                byte* numPtr3 = numPtr1;
                byte* numPtr4 = numPtr2;
                do
                    ;
                while ((int)*numPtr3++ == (int)*numPtr4++ && num < length - 1 && ++num > 0);
            }
            return length - 1 == num;
        }

        public static string BytesToString(byte[] buffer, int offset, int length)
        {
            if (length == 0)
                return "";
            return BitConverter.ToString(buffer, offset, length).Replace("-", " ");
        }

        public static uint RandomUINT()
        {
            byte[] buffer = new byte[4];
            Utils.m_Random.NextBytes(buffer);
            return (uint)((int)buffer[0] | (int)buffer[1] << 8 | (int)buffer[2] << 16 | (int)buffer[3] << 24);
        }

        public static int RandomRange(int min, int max)
        {
            if (min > max)
            {
                int num = min;
                min = max;
                max = num;
            }
            else if (min == max)
                return min;
            return min + Utils.m_Random.Next(max - min + 1);
        }

        public static bool RandomBool()
        {
            return Utils.m_Random.Next(2) == 0;
        }

        public static void DisplayPacket(byte[] buffer)
        {
            TextWriter output = (TextWriter)new StringWriter();
            MemoryStream memoryStream = new MemoryStream(buffer);
            Utils.FormatBuffer(output, (Stream)memoryStream, (int)memoryStream.Length);
            Console.WriteLine(output.ToString());
        }

        public static byte[] GetBytes(ushort u)
        {
            byte[] numArray = new byte[2];
            for (int index = 1; index <= 2; ++index)
                numArray[index - 1] = (byte)((uint)u >> 16 - index * 8);
            return numArray;
        }

        public static byte[] GetBytes(long s)
        {
            byte[] numArray = new byte[8];
            ulong num = (ulong)s;
            for (int index = 1; index <= 8; ++index)
                numArray[index - 1] = (byte)(num >> 64 - index * 8);
            return numArray;
        }

        public static byte[] GetBytes(uint t)
        {
            byte[] numArray = new byte[4];
            for (int index = 1; index <= 4; ++index)
                numArray[index - 1] = (byte)(t >> 32 - index * 8);
            return numArray;
        }

        public static void LogFailed(string data)
        {
            try
            {
                Utils.Log(data);
            }
            catch
            {
            }
        }

        public static string DateTimeStamp()
        {
            return "[" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "]";
        }
    }
}
