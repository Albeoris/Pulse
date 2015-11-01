using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ZtrFileTextPacker
    {
        private readonly Stream _output;
        private readonly ZtrFileEntry[] _input;
        private readonly FFXIIITextEncoding _encoding;

        private int _blockOffset;
        private byte _blockNumber;
        private int _innerIndex;
        private int _innerCount;

        public ZtrFileTextPacker(Stream output, ZtrFileEntry[] input, FFXIIITextEncoding encoding)
        {
            _output = output;
            _input = input;
            _encoding = encoding;
        }

        public unsafe void Pack(ZtrFileHeader header)
        {
            _blockOffset = 0;
            _blockNumber = 0;
            _innerIndex = 0;
            _innerCount = 0;

            List<int> blockOffsets = new List<int>(256) {0};
            ZtrFileHeaderLineInfo[] lines = new ZtrFileHeaderLineInfo[_input.Length];
            ushort[,] innerOffsets = new ushort[_input.Length, 2];

            ushort writeIndex = 0;
            byte[] writeBuff = new byte[4096];
            byte[] codeBuff = new byte[64 * 1024];

            fixed (byte* writeBuffPtr = &writeBuff[0])
            fixed (byte* codeBuffPtr = &codeBuff[0])
            {
                for (int e = 0; e < _input.Length; e++)
                {
                    _innerCount++;
                    innerOffsets[e, 0] = writeIndex;

                    ZtrFileEntry entry = _input[e];
                    int count = _encoding.GetBytes(entry.Value, 0, entry.Value.Length, codeBuff, 0);
                    codeBuff[count++] = 0;
                    codeBuff[count++] = 0;

                    lines[e] = new ZtrFileHeaderLineInfo
                    {
                        Block = _blockNumber,
                        BlockOffset = 0 // See below: lines[i].BlockOffset = checked ((byte)innerOffsets[i, 1]);
                    };

                    for (int b = 0; b < count; b++)
                    {
                        writeBuffPtr[writeIndex++] = codeBuffPtr[b];
                        if (writeIndex == writeBuff.Length)
                            WriteBlock(writeBuff, ref writeIndex, innerOffsets, blockOffsets);
                    }
                }
            }

            if (writeIndex > 0)
                WriteBlock(writeBuff, ref writeIndex, innerOffsets, blockOffsets);

            for (int i = 0; i < innerOffsets.Length / 2; i++)
            {
                lines[i].PackedOffset = innerOffsets[i, 0];
                lines[i].BlockOffset = checked ((byte)innerOffsets[i, 1]);
            }

            if (blockOffsets.Count > _blockNumber)
                _blockNumber++;

            header.TextBlocksCount = _blockNumber;
            header.TextBlockTable = blockOffsets.ToArray();
            header.TextLinesTable = lines;
        }

        private void WriteBlock(byte[] writeBuff, ref ushort engaged, ushort[,] innerOffsets, List<int> blockOffsets)
        {
            byte[] encoding = ZtrFileEncoding.FakeCompression(writeBuff, 0, ref engaged, innerOffsets, _innerIndex, _innerCount);
            _output.Write(encoding, 0, encoding.Length);
            _output.Write(writeBuff, 0, engaged);

            _blockOffset += encoding.Length;
            _blockOffset += engaged;
            _blockNumber = checked((byte)(_blockNumber + 1));

            _innerIndex += _innerCount;
            _innerCount = 0;

            blockOffsets.Add(_blockOffset);
            engaged = 0;
        }
    }
}