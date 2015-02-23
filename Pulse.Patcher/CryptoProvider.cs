using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pulse.Core;

namespace Pulse.Patcher
{
    internal sealed class CryptoProvider : IDisposable
    {
        private readonly AesCryptoServiceProvider _aes;
        private ManualResetEvent _cancelEvent;

        public event Action<long> Progress;

        public CryptoProvider(string securityKey, ManualResetEvent cancelEvent)
        {
            _cancelEvent = cancelEvent;

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

        public async Task Encrypt(Stream input, Stream output)
        {
            _aes.GenerateIV();

            if (_cancelEvent.IsSet())
                return;

            byte[] vector = _aes.IV;
            byte[] vectorLength = BitConverter.GetBytes(vector.Length);
            output.Write(vectorLength, 0, 4);
            output.Write(vector, 0, vector.Length);

            if (_cancelEvent.IsSet())
                return;

            using (ICryptoTransform encryptor = _aes.CreateEncryptor())
            using (CryptoStream encryptionStream = new CryptoStream(output, encryptor, CryptoStreamMode.Write))
            {
                if (_cancelEvent.IsSet())
                    return;

                await PatcherService.CopyAsync(input, encryptionStream, _cancelEvent, Progress);
                encryptionStream.FlushFinalBlock();
            }
        }

        public async Task Decrypt(Stream input, Stream output)
        {
            _aes.IV = input.EnsureRead(BitConverter.ToInt32(input.EnsureRead(4), 0));
            Progress.NullSafeInvoke(4);

            using (ICryptoTransform decryptor = _aes.CreateDecryptor())
            using (CryptoStream encryptionStream = new CryptoStream(input, decryptor, CryptoStreamMode.Read))
            {
                if (_cancelEvent.IsSet())
                    return;

                await PatcherService.CopyAsync(encryptionStream, output, _cancelEvent, Progress);
            }
        }
    }
}