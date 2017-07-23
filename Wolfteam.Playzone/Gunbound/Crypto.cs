using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Wolfteam.Playzone.Util;

namespace Wolfteam.Playzone.Gunbound
{
    /// <summary>
    ///     Modified file from https://github.com/CarlosX/GunBoundWC.
    /// </summary>
    public class Crypto
    {
        private static byte[] m_StaticKey = new byte[16]
        {
            (byte) 169,
            (byte) 39,
            (byte) 83,
            (byte) 4,
            (byte) 27,
            (byte) 252,
            (byte) 172,
            (byte) 230,
            (byte) 91,
            (byte) 35,
            (byte) 56,
            (byte) 52,
            (byte) 104,
            (byte) 70,
            (byte) 3,
            (byte) 140
        };
        private static Rijndael m_StaticRijndael;
        private Rijndael m_DynamicRijndael;

        // public Crypto(string login, string pass, uint dword)
        public Crypto(string aeskey)
        {
            // pass = "aeria_bc";

            // if (login.Length > 16 || pass.Length > 20)
            //     return;
            // MD5 md5 = MD5.Create();
            // SHA1CryptoServiceProvider cryptoServiceProvider = new SHA1CryptoServiceProvider();
            // SpecialSHA specialSha = new SpecialSHA();

//            string str = "";
//            byte[] bytes = Utils.GetBytes(dword);
//            for (int index = bytes.Length - 1; index >= 0; --index)
//                str += specialSha.Chr(bytes[index]).ToString();
//            string inMsg = login + pass + dword;
//            byte[] numArray = specialSha.SHA1(inMsg);

            this.m_DynamicRijndael = Rijndael.Create();
            this.m_DynamicRijndael.Key = HexUtil.StringToByteArray(aeskey); //numArray;
            this.m_DynamicRijndael.Mode = CipherMode.ECB;
            this.m_DynamicRijndael.Padding = PaddingMode.Zeros;
        }

        public static void Initialize()
        {
            Crypto.m_StaticRijndael = Rijndael.Create();
            Crypto.m_StaticRijndael.Key = Crypto.m_StaticKey;
            Crypto.m_StaticRijndael.Mode = CipherMode.ECB;
            Crypto.m_StaticRijndael.Padding = PaddingMode.Zeros;
        }

        private static byte[] DecryptStatic(byte[] cipherData)
        {
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, Crypto.m_StaticRijndael.CreateDecryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(cipherData, 0, cipherData.Length);
            cryptoStream.Close();
            return memoryStream.ToArray();
        }

        private static byte[] EncryptStatic(byte[] rawData)
        {
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, Crypto.m_StaticRijndael.CreateEncryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(rawData, 0, rawData.Length);
            cryptoStream.Close();
            return memoryStream.ToArray();
        }

        private byte[] DecryptDynamic(byte[] cipherData)
        {
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, this.m_DynamicRijndael.CreateDecryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(cipherData, 0, cipherData.Length);
            cryptoStream.Close();
            return memoryStream.ToArray();
        }

        public static byte[] DecryptStaticBuffer(byte[] decryptme)
        {
            if (decryptme.Length == 16)
                return Crypto.DecryptStatic(decryptme);

            throw new Exception("DecryptStatic() is 128-bit only.");
        }

        public static byte[] EncryptStaticBuffer(byte[] encryptme)
        {
            if (encryptme.Length == 16)
                return Crypto.EncryptStatic(encryptme);
            
            throw new Exception("EncryptStatic() is 128-bit only.");
        }

        public void Dispose()
        {
            this.m_DynamicRijndael = (Rijndael)null;
        }

        public bool PacketDecrypt(byte[] input, ref byte[] output, ushort packetid)
        {
            if (input.Length == 0)
            {
                Utils.Log("Empty buffer passed for decryption");
                return false;
            }
            if (input.Length % 16 != 0)
            {
                Utils.Log("Decrypt failed. Input byte count is not a multiple of 16.");
                return false;
            }
            int num1 = input.Length / 16;
            int length = num1 * 12;
            byte[] numArray1 = new byte[input.Length];
            byte[] numArray2 = new byte[length];
            byte[] buffer = this.DecryptDynamic(input);
            Console.WriteLine(BitConverter.ToString(buffer));
            PacketReader packetReader = new PacketReader(buffer, buffer.Length);
            uint num2 = 2831345089;
            for (int index1 = 0; index1 < num1; ++index1)
            {
                packetReader.Seek(16 * index1, SeekOrigin.Begin);
                uint num3 = packetReader.ReadUInt32();
                if ((int) num3 - (int) packetid != (int) num2)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                Console.WriteLine($"{num3,-10} - {packetid} != {num2} ({num3 - packetid,-10} != {num2})");
                Console.ResetColor();
                if ((int)num3 - (int)packetid != (int)num2)
                {
                    // Console.WriteLine("Bad Packet Signature. G: {0,8:X8} E: {1,8:X8}", (object)num3, (object)(uint)((int)num2 + (int)packetid));
                    return false;
                }
                for (int index2 = 4; index2 < 16; ++index2)
                    numArray2[index1 * 12 + (index2 - 4)] = buffer[index1 * 16 + index2];
            }
            output = numArray2;
            return true;
        }
    }
}
