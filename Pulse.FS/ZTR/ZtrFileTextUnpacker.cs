using System;
using System.IO;

namespace Pulse.FS
{
    public sealed class ZtrFileTextUnpacker
    {
        private Stream _input;
        private Stream _output;

        public ZtrFileTextUnpacker(Stream input, Stream output)
        {
            _input = input;
            _output = output;
        }

        public void Unpack(int compressedSize)
        {
            byte[] buff = new byte[4096];
            while (compressedSize > 0)
            {
                int offset = 0;
                ZtrFileEncoding tagsEncoding = ZtrFileEncoding.ReadFromStream(_input);
                compressedSize = compressedSize - tagsEncoding.BlockSize - 4;
                while (offset < 4096 && compressedSize > 0)
                {
                    int value = _input.ReadByte();
                    byte[] replace = tagsEncoding.Encoding[value];

                    Array.Copy(replace, 0, buff, offset, replace.Length);

                    offset += replace.Length;
                    compressedSize--;
                }
                _output.Write(buff, 0, offset);
            }
        }
    }
}