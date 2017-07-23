using System;

namespace Wolfteam.Playzone.Gunbound
{
    /// <summary>
    ///     File from https://github.com/CarlosX/GunBoundWC.
    /// </summary>
    public class SpecialSHA
    {
        private byte[] m_ByteArray;
        private long m_HighByte;
        private long m_HighBound;

        private void Reset()
        {
            this.m_HighByte = 0L;
            this.m_HighBound = 1024L;
            this.m_ByteArray = new byte[1024];
        }

        private SpecialSHA.DWORD AndW(SpecialSHA.DWORD w1, SpecialSHA.DWORD w2)
        {
            return new SpecialSHA.DWORD()
            {
                B0 = (byte)((uint)w1.B0 & (uint)w2.B0),
                B1 = (byte)((uint)w1.B1 & (uint)w2.B1),
                B2 = (byte)((uint)w1.B2 & (uint)w2.B2),
                B3 = (byte)((uint)w1.B3 & (uint)w2.B3)
            };
        }

        private SpecialSHA.DWORD OrW(SpecialSHA.DWORD w1, SpecialSHA.DWORD w2)
        {
            return new SpecialSHA.DWORD()
            {
                B0 = (byte)((uint)w1.B0 | (uint)w2.B0),
                B1 = (byte)((uint)w1.B1 | (uint)w2.B1),
                B2 = (byte)((uint)w1.B2 | (uint)w2.B2),
                B3 = (byte)((uint)w1.B3 | (uint)w2.B3)
            };
        }

        private SpecialSHA.DWORD XOrW(SpecialSHA.DWORD w1, SpecialSHA.DWORD w2)
        {
            return new SpecialSHA.DWORD()
            {
                B0 = (byte)((uint)w1.B0 ^ (uint)w2.B0),
                B1 = (byte)((uint)w1.B1 ^ (uint)w2.B1),
                B2 = (byte)((uint)w1.B2 ^ (uint)w2.B2),
                B3 = (byte)((uint)w1.B3 ^ (uint)w2.B3)
            };
        }

        private SpecialSHA.DWORD NotW(SpecialSHA.DWORD w)
        {
            SpecialSHA.DWORD dword;
            dword.B0 = w.B0;
            dword.B1 = w.B1;
            dword.B2 = w.B2;
            dword.B3 = w.B3;
            return dword;
        }

        private void Append(byte data)
        {
            if (1L + this.m_HighByte > this.m_HighBound)
                this.m_HighBound += 1024L;
            this.m_ByteArray[this.m_HighByte] = data;
            ++this.m_HighByte;
        }

        private void Append(string data)
        {
            long length = (long)data.Length;
            if (length + this.m_HighByte > this.m_HighBound)
                this.m_HighBound += 1024L;
            byte[] bytes = Utils.GetBytes(data);
            for (int index = 0; (long)index < length; ++index)
                this.m_ByteArray[this.m_HighByte + (long)index] = bytes[index];
            this.m_HighByte += length;
        }

        public char Chr(byte i)
        {
            return Utils.Encoding.GetChars(new byte[1] { i }, 0, 1)[0];
        }

        public string String(long times, string repeat)
        {
            string str = "";
            for (long index = 0; index < times; ++index)
                str += repeat;
            return str;
        }

        private string Space(long count)
        {
            string str = "";
            for (int index = 0; (long)index < count; ++index)
                str += " ";
            return str;
        }

        private byte[] GData()
        {
            byte[] numArray = new byte[this.m_HighByte];
            for (int index = 0; (long)index < this.m_HighByte; ++index)
                numArray[index] = this.m_ByteArray[index];
            return numArray;
        }

        private SpecialSHA.DWORD CircShiftLeftW(SpecialSHA.DWORD w, int n)
        {
            uint num1 = this.DWORDToUINT(w);
            uint num2 = num1;
            return this.OrW(this.ToDWORD((uint)((double)num1 * Math.Pow(2.0, (double)n))), this.ToDWORD((uint)((double)num2 / Math.Pow(2.0, (double)(32 - n)))));
        }

        private SpecialSHA.DWORD AddW(SpecialSHA.DWORD w1, SpecialSHA.DWORD w2)
        {
            int num1 = (int)w1.B3 + (int)w2.B3;
            SpecialSHA.DWORD dword;
            dword.B3 = (byte)(num1 % 256);
            int num2 = (int)w1.B2 + (int)w2.B2 + num1 / 256;
            dword.B2 = (byte)(num2 % 256);
            int num3 = (int)w1.B1 + (int)w2.B1 + num2 / 256;
            dword.B1 = (byte)(num3 % 256);
            int num4 = (int)w1.B0 + (int)w2.B0 + num3 / 256;
            dword.B0 = (byte)(num4 % 256);
            return dword;
        }

        public byte[] SHA1(string inMsg)
        {
            SpecialSHA.DWORD[] dwordArray1 = new SpecialSHA.DWORD[4];
            SpecialSHA.DWORD[] dwordArray2 = new SpecialSHA.DWORD[5];
            SpecialSHA.DWORD[] dwordArray3 = new SpecialSHA.DWORD[80];
            long length = (long)inMsg.Length;
            SpecialSHA.DWORD dword1 = this.ToDWORD((uint)((ulong)length * 8UL));
            this.Reset();
            int num1 = (int)(128L - length % 64L - 9L) % 64;
            int num2 = inMsg.Length + 9 + num1;
            this.Append(inMsg);
            this.Append((byte)128);
            for (int index = 0; index < num1 + 4; ++index)
                this.Append((byte)0);
            this.Append(dword1.B0);
            this.Append(dword1.B1);
            this.Append(dword1.B2);
            this.Append(dword1.B3);
            byte[] numArray1 = this.GData();
            this.Reset();
            long num3 = (long)(numArray1.Length / 64);
            dwordArray1[0] = this.ToDWORD(1518500249U);
            dwordArray1[1] = this.ToDWORD(1859775393U);
            dwordArray1[2] = this.ToDWORD(2400959708U);
            dwordArray1[3] = this.ToDWORD(3395469782U);
            dwordArray2[0] = this.ToDWORD(1732584193U);
            dwordArray2[1] = this.ToDWORD(4023233417U);
            dwordArray2[2] = this.ToDWORD(2562383102U);
            dwordArray2[3] = this.ToDWORD(271733878U);
            dwordArray2[4] = this.ToDWORD(3285377520U);
            for (int index1 = 0; (long)index1 < num3; ++index1)
            {
                byte[] numArray2 = new byte[64];
                for (int index2 = index1 * 64; index2 < (index1 + 1) * 64; ++index2)
                    numArray2[index2 % 64] = numArray1[index2];
                for (int index2 = 0; index2 <= 15; ++index2)
                {
                    dwordArray3[index2].B0 = numArray2[index2 * 4];
                    dwordArray3[index2].B1 = numArray2[index2 * 4 + 1];
                    dwordArray3[index2].B2 = numArray2[index2 * 4 + 2];
                    dwordArray3[index2].B3 = numArray2[index2 * 4 + 3];
                }
                for (int index2 = 16; index2 <= 79; ++index2)
                    dwordArray3[index2] = this.XOrW(this.XOrW(this.XOrW(dwordArray3[index2 - 3], dwordArray3[index2 - 8]), dwordArray3[index2 - 14]), dwordArray3[index2 - 16]);
                SpecialSHA.DWORD dword2 = dwordArray2[0];
                SpecialSHA.DWORD dword3 = dwordArray2[1];
                SpecialSHA.DWORD dword4 = dwordArray2[2];
                SpecialSHA.DWORD dword5 = dwordArray2[3];
                SpecialSHA.DWORD w2 = dwordArray2[4];
                for (int t = 0; t <= 79; ++t)
                {
                    SpecialSHA.DWORD dword6 = this.AddW(this.AddW(this.AddW(this.AddW(this.CircShiftLeftW(dword2, 5), this.F(t, dword3, dword4, dword5)), w2), dwordArray3[t]), dwordArray1[t / 20]);
                    w2 = dword5;
                    dword5 = dword4;
                    dword4 = this.CircShiftLeftW(dword3, 30);
                    dword3 = dword2;
                    dword2 = dword6;
                }
                dwordArray2[0] = this.AddW(dwordArray2[0], dword2);
                dwordArray2[1] = this.AddW(dwordArray2[1], dword3);
                dwordArray2[2] = this.AddW(dwordArray2[2], dword4);
                dwordArray2[3] = this.AddW(dwordArray2[3], dword5);
                dwordArray2[4] = this.AddW(dwordArray2[4], w2);
            }
            return new byte[16]
            {
                dwordArray2[0].B3,
                dwordArray2[0].B2,
                dwordArray2[0].B1,
                dwordArray2[0].B0,
                dwordArray2[1].B3,
                dwordArray2[1].B2,
                dwordArray2[1].B1,
                dwordArray2[1].B0,
                dwordArray2[2].B3,
                dwordArray2[2].B2,
                dwordArray2[2].B1,
                dwordArray2[2].B0,
                dwordArray2[3].B3,
                dwordArray2[3].B2,
                dwordArray2[3].B1,
                dwordArray2[3].B0
            };
        }

        private uint DWORDToUINT(SpecialSHA.DWORD w)
        {
            return (uint)((int)w.B0 << 24 | (int)w.B1 << 16 | (int)w.B2 << 8) | (uint)w.B3;
        }

        private SpecialSHA.DWORD ToDWORD(uint n)
        {
            SpecialSHA.DWORD dword;
            dword.B0 = (byte)(n >> 24);
            dword.B1 = (byte)(n >> 16);
            dword.B2 = (byte)(n >> 8);
            dword.B3 = (byte)n;
            return dword;
        }

        private SpecialSHA.DWORD F(int t, SpecialSHA.DWORD b, SpecialSHA.DWORD c, SpecialSHA.DWORD d)
        {
            if (t <= 19)
                return this.OrW(this.AndW(b, c), this.AndW(this.NotW(b), d));
            if (t <= 39 || t > 59)
                return this.XOrW(this.XOrW(b, c), d);
            return this.OrW(this.OrW(this.AndW(b, c), this.AndW(b, d)), this.AndW(c, d));
        }

        private struct DWORD
        {
            public byte B0;
            public byte B1;
            public byte B2;
            public byte B3;
        }
    }
}
