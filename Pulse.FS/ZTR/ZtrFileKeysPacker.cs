using System;
using System.IO;
using System.Text;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ZtrFileKeysPacker
    {
        private readonly Stream _output;
        private readonly ZtrFileEntry[] _input;

        public ZtrFileKeysPacker(Stream output, ZtrFileEntry[] input)
        {
            _output = output;
            _input = input;
        }

        public unsafe void Pack(ZtrFileHeader header)
        {
            int uncompressedSize = 0;

            ushort index = 0;
            byte[] writeBuff = new byte[4096];
            byte[] codeBuff = new byte[256];

            fixed (byte* writeBuffPtr = &writeBuff[0])
            fixed (byte* codeBuffPtr = &codeBuff[0])
            {
                for (int e = 0; e < _input.Length; e++)
                {
                    ZtrFileEntry entry = _input[e];
                    int count = Encoding.ASCII.GetBytes(entry.Key, 0, entry.Key.Length, codeBuff, 0);
                    codeBuff[count++] = 0;

                    for (int b = 0; b < count; b++)
                    {
                        writeBuffPtr[index++] = codeBuffPtr[b];
                        if (index == writeBuff.Length)
                            WriteBlock(writeBuff, ref index, ref uncompressedSize);
                    }
                }
            }

            if (index > 0)
                WriteBlock(writeBuff, ref index, ref uncompressedSize);

            header.KeysUnpackedSize = uncompressedSize;
        }

        private void WriteBlock(byte[] writeBuff, ref ushort engaged, ref int uncompressedSize)
        {
            uncompressedSize += engaged;

            byte[] encoding = ZtrFileEncoding.CompressZtrContent(writeBuff, 0, ref engaged);
            _output.Write(encoding, 0, encoding.Length);
            _output.Write(writeBuff, 0, engaged);

            engaged = 0;
        }
    }
}