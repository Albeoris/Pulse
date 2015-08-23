using System;
using System.Collections.Generic;
using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ZtrFileTextPacker
    {
        private readonly Stream _output;
        private readonly ZtrFileEntry[] _input;
        private readonly ZtrFileHeader _header;
        private readonly FFXIIITextEncoding _encoding;

        public ZtrFileTextPacker(Stream output, ZtrFileEntry[] input, ZtrFileHeader header, FFXIIITextEncoding encoding)
        {
            _output = output;
            _input = input;
            _header = header;
            _encoding = encoding;
        }

        public void Pack()
        {
            byte blockIndex = 0;
            List<int> blockOffsets = new List<int>(256) {0};
            ZtrFileHeaderLineInfo[] infos = new ZtrFileHeaderLineInfo[_input.Length];

            //int offset = 0;
            //for (int i = 0; i < _input.Length; i++)
            //{
            //    String value = _input[i].Value;
            //    ZtrFileHeaderLineInfo info = new ZtrFileHeaderLineInfo();
            //    info.Block = blockIndex;
            //    info.BlockOffset = offset;
            //}


            //int index = 0, blockNumber = 0;
            //using (MemoryStream io = new MemoryStream(32768))
            //{
            //    byte[] readBuff = new byte[4096];
            //    while (compressedSize > 0)
            //    {
            //        int offset = 0;
            //        ZtrFileEncoding tagsEncoding = ZtrFileEncoding.ReadFromStream(_output);
            //        compressedSize = compressedSize - tagsEncoding.BlockSize - 4;
            //        long blockOffset = _output.Position;
            //        while (offset < 4096 && compressedSize > 0)
            //        {
            //            while (index < _offsets.Length && _offsets[index].Block == blockNumber && _output.Position - blockOffset == _offsets[index].PackedOffset)
            //            {
            //                _offsets[index].UnpackedOffset = (int)(io.Position + offset + _offsets[index].BlockOffset);
            //                if (index > 0)
            //                    _offsets[index - 1].UnpackedLength = _offsets[index].UnpackedOffset - _offsets[index - 1].UnpackedOffset;
            //                index++;
            //            }

            //            int value = _output.ReadByte();
            //            byte[] replace = tagsEncoding.Encoding[value];

            //            Array.Copy(replace, 0, readBuff, offset, replace.Length);

            //            offset += replace.Length;
            //            compressedSize--;
            //        }

            //        io.Write(readBuff, 0, offset);
            //        blockNumber++;
            //    }

            //    _offsets[index - 1].UnpackedLength = (int)(io.Position - _offsets[index - 1].UnpackedOffset);
            //    for (int i = 0; i < _offsets.Length; i++)
            //    {
            //        io.SetPosition(_offsets[i].UnpackedOffset);
            //        byte[] buff = io.EnsureRead(_offsets[i].UnpackedLength);
            //        _input[i].Value = _encoding.GetString(buff, 0, buff.Length - 2);
            //    }
            //}
        }
    }
}