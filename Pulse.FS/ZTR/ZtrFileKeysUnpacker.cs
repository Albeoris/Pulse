using System;
using System.IO;
using System.Text;

namespace Pulse.FS
{
    public sealed class ZtrFileKeysUnpacker
    {
        private Stream _input;
        private ZtrFileEntry[] _output;

        public ZtrFileKeysUnpacker(Stream input, ZtrFileEntry[] output)
        {
            _input = input;
            _output = output;
        }

        public void Unpack(int uncompressedSize)
        {
            int index = 0, lastOffset = 0;
            byte[] readBuff = new byte[Math.Min(uncompressedSize, 4096)];
            byte[] codeBuff = new byte[readBuff.Length * 2];
            while (uncompressedSize > 0)
            {
                int offset = 0;
                ZtrFileEncoding tagsEncoding = ZtrFileEncoding.ReadFromStream(_input);
                while (offset < 4096 && uncompressedSize > 0)
                {
                    int value = _input.ReadByte();
                    byte[] replace = tagsEncoding.Encoding[value];

                    Array.Copy(replace, 0, readBuff, offset, replace.Length);

                    offset += replace.Length;
                    uncompressedSize -= replace.Length;
                }

                // Выбираем полные строки
                Array.Copy(readBuff, 0, codeBuff, lastOffset, offset);

                int initial = 0, length = offset + lastOffset;
                for (int i = lastOffset; i < length; i++)
                {
                    if (codeBuff[i] == 0)
                    {
                        _output[index++].Key = Encoding.ASCII.GetString(codeBuff, initial, i - initial);
                        initial = i + 1;
                    }
                }

                if (initial == 0) throw new NotImplementedException();

                lastOffset = length - initial;
                Array.Copy(codeBuff, initial, codeBuff, 0, lastOffset);
            }
        }
    }
}