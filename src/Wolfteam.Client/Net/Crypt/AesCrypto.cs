using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Wolfteam.Client.Net.Crypt
{
    /// <summary>
    ///     Thanks to https://github.com/CarlosX for his GunBoundWC project (https://github.com/CarlosX/GunBoundWC).
    ///     I have added encrypt methods because we have to work from client to server. :)
    /// </summary>
    internal class AesCrypto : IDisposable
    {
        /// <summary>
        ///     Static AES key obtained from memory of Wolfteam by https://github.com/CarlosX/GunBoundWC.
        ///     See the README.md for more information.
        /// </summary>
        private static readonly byte[] StaticKey = {0xA9, 0x27, 0x53, 0x04, 0x1B, 0xFC, 0xAC, 0xE6, 0x5B, 0x23, 0x38, 0x34, 0x68, 0x46, 0x03, 0x8C};

        private static readonly Rijndael StaticRijndael;

        private readonly Rijndael _dynamicRijndael;

        public AesCrypto(byte[] key)
        {
            _dynamicRijndael = Rijndael.Create();
            _dynamicRijndael.Key = key;
            _dynamicRijndael.Mode = CipherMode.ECB;
            _dynamicRijndael.Padding = PaddingMode.Zeros;
        }

        public async Task<byte[]> EncryptAsync(byte[] buffer)
        {
            return await InternalEncryptAsync(_dynamicRijndael, buffer);
        }

        public async Task<byte[]> DecryptAsync(byte[] buffer)
        {
            return await InternalDecryptAsync(_dynamicRijndael, buffer);
        }

        public void Dispose()
        {
            _dynamicRijndael?.Dispose();
        }

        #region Static crypto
        static AesCrypto()
        {
            StaticRijndael = Rijndael.Create();
            StaticRijndael.Key = StaticKey;
            StaticRijndael.Mode = CipherMode.ECB;
            StaticRijndael.Padding = PaddingMode.Zeros;
        }

        public static async Task<byte[]> StaticDecryptAsync(byte[] buffer)
        {
            if (buffer.Length % 16 != 0)
            {
                throw new Exception($"{nameof(buffer)} must be a multiple of 16.");
            }

            return await InternalDecryptAsync(StaticRijndael, buffer);
        }

        public static async Task<byte[]> StaticEncryptAsync(byte[] buffer)
        {
            if (buffer.Length % 16 != 0)
            {
                throw new Exception($"{nameof(buffer)} must be a multiple of 16.");
            }

            return await InternalEncryptAsync(StaticRijndael, buffer);
        }

        private static async Task<byte[]> InternalDecryptAsync(SymmetricAlgorithm rijndael, byte[] buffer)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, rijndael.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    await cryptoStream.WriteAsync(buffer, 0, buffer.Length);
                    cryptoStream.Close();
                    return memoryStream.ToArray();
                }
            }
        }

        private static async Task<byte[]> InternalEncryptAsync(SymmetricAlgorithm rijndael, byte[] buffer)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, rijndael.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    await cryptoStream.WriteAsync(buffer, 0, buffer.Length);
                    cryptoStream.Close();
                    return memoryStream.ToArray();
                }
            }
        }
        #endregion
    }
}
