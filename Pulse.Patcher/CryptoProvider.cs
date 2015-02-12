using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Pulse.Core;

namespace Pulse.Patcher
{
    internal sealed class CryptoProvider : IDisposable
    {
        private readonly AesCryptoServiceProvider _aes;

        public CryptoProvider(string securityKey)
        {
            byte[] key = GenerateKey(securityKey);
            _aes = new AesCryptoServiceProvider {KeySize = key.Length * 8, Key = key};
        }

        public void Dispose()
        {
            _aes.Dispose();
        }

        private static byte[] GenerateKey(string securityKey)
        {
            byte[] buff = Encoding.UTF8.GetBytes(securityKey);
            using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
                return sha256.ComputeHash(buff);
        }

        public void Encrypt(Stream input, Stream output)
        {
            _aes.GenerateIV();

            byte[] vector = _aes.IV;
            byte[] vectorLength = BitConverter.GetBytes(vector.Length);
            output.Write(vectorLength, 0, 4);
            output.Write(vector, 0, vector.Length);

            using (ICryptoTransform encryptor = _aes.CreateEncryptor())
            using (CryptoStream encryptionStream = new CryptoStream(output, encryptor, CryptoStreamMode.Write))
            {
                input.CopyTo(encryptionStream);
                encryptionStream.FlushFinalBlock();
            }
        }

        public void Decrypt(Stream input, Stream output)
        {
            _aes.IV = input.EnsureRead(BitConverter.ToInt32(input.EnsureRead(4), 0));

            using (ICryptoTransform decryptor = _aes.CreateDecryptor())
            using (CryptoStream encryptionStream = new CryptoStream(input, decryptor, CryptoStreamMode.Read))
                encryptionStream.CopyTo(output);
        }
    }
}