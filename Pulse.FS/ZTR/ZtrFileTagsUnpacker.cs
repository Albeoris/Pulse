using System;
using System.IO;

namespace Pulse.FS
{
    public sealed class ZtrFileTagsUnpacker
    {
        private Stream _input;
        private Stream _output;

        public ZtrFileTagsUnpacker(Stream input, Stream output)
        {
            _input = input;
            _output = output;
        }

        public void Unpack(int uncompressedSize)
        {
            byte[] buff = new byte[Math.Min(uncompressedSize, 4096)];
            while (uncompressedSize > 0)
            {
                int offset = 0;
                ZtrFileEncoding tagsEncoding = ZtrFileEncoding.ReadFromStream(_input);
                while (offset < 4096 && uncompressedSize > 0)
                {
                    int value = _input.ReadByte();
                    byte[] replace = tagsEncoding.Encoding[value];

                    Array.Copy(replace, 0, buff, offset, replace.Length);

                    offset += replace.Length;
                    uncompressedSize -= replace.Length;
                }
                _output.Write(buff, 0, offset);
            }
        }
    }
}