using System;
using System.Collections.Generic;
using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ZtrFileEncoding
    {
        public readonly int BlockSize;
        public readonly byte[][] Encoding;

        private ZtrFileEncoding(int blockSize, byte[][] result)
        {
            BlockSize = blockSize;
            Encoding = result;
        }

        public static unsafe ZtrFileEncoding ReadFromStream(Stream input)
        {
            int blockSize;
            byte[] buff = input.EnsureRead(4);
            fixed (byte* b = &buff[0])
                blockSize = Endian.ToLittleInt32(b);

            byte[] values = new byte[blockSize / 3];
            byte[,] encoding = new byte[256, 2];
            if (blockSize > 0)
            {
                buff = input.EnsureRead(blockSize);
                fixed (byte* b = &buff[0])
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        byte value = *(b + i * 3);
                        encoding[value, 0] = *(b + i * 3 + 1);
                        encoding[value, 1] = *(b + i * 3 + 2);
                        values[i] = value;
                    }
                }
            }

            List<byte>[] lists = new List<byte>[256];
            for (int i = 0; i < 256; i++)
            {
                List<byte> list = lists[i] = new List<byte>(16);
                DecodeByte(values, encoding, (byte)i, list);
            }

            byte[][] result = new byte[256][];
            for (int i = 0; i < 256; i++)
                result[i] = lists[i].ToArray();

            return new ZtrFileEncoding(blockSize, result);
        }

        private static void DecodeByte(byte[] knownValues, byte[,] encoding, byte value, List<byte> list)
        {
            if (Array.IndexOf(knownValues, value) < 0)
            {
                list.Add(value);
            }
            else
            {
                DecodeByte(knownValues, encoding, encoding[value, 0], list);
                DecodeByte(knownValues, encoding, encoding[value, 1], list);
            }
        }
    }
}