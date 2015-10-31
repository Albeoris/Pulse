using System;
using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ZtrFileTextUnpacker
    {
        private readonly Stream _input;
        private readonly ZtrFileEntry[] _output;
        private readonly ZtrFileHeaderLineInfo[] _offsets;
        private readonly FFXIIITextEncoding _encoding;

        public ZtrFileTextUnpacker(Stream input, ZtrFileEntry[] output, ZtrFileHeaderLineInfo[] offsets, FFXIIITextEncoding encoding)
        {
            _input = input;
            _output = output;
            _offsets = offsets;
            _encoding = encoding;
        }
        
        public void Unpack(int compressedSize)
        {
            if (compressedSize < 1)
                return;

            int index = 0, blockNumber = 0;
            using (MemoryStream io = new MemoryStream(32768))
            {
                byte[] readBuff = new byte[4096];
                while (compressedSize > 0)
                {
                    int offset = 0;
                    ZtrFileEncoding tagsEncoding = ZtrFileEncoding.ReadFromStream(_input);
                    compressedSize = compressedSize - tagsEncoding.BlockSize - 4;
                    long blockOffset = _input.Position;
                    while (offset < 4096 && compressedSize > 0)
                    {
                        while (index < _offsets.Length && _offsets[index].Block == blockNumber && _input.Position - blockOffset == _offsets[index].PackedOffset)
                        {
                            _offsets[index].UnpackedOffset = (int)(io.Position + offset + _offsets[index].BlockOffset);
                            if (index > 0)
                                _offsets[index - 1].UnpackedLength = _offsets[index].UnpackedOffset - _offsets[index - 1].UnpackedOffset;
                            index++;
                        }

                        int value = _input.ReadByte();
                        byte[] replace = tagsEncoding.Encoding[value];

                        Array.Copy(replace, 0, readBuff, offset, replace.Length);

                        offset += replace.Length;
                        compressedSize--;
                    }

                    io.Write(readBuff, 0, offset);
                    blockNumber++;
                }

                _offsets[index - 1].UnpackedLength = (int)(io.Position - _offsets[index - 1].UnpackedOffset);
                for (int i = 0; i < _offsets.Length; i++)
                {
                    io.SetPosition(_offsets[i].UnpackedOffset);
                    byte[] buff = io.EnsureRead(_offsets[i].UnpackedLength);
                    _output[i].Value = _encoding.GetString(buff, 0, buff.Length - 2);
                }
            }
        }
    }
}