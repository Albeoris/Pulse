using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ZtrFileEncoding
    {
        public readonly int BlockSize;
        public readonly byte[][] Encoding;

        private ZtrFileEncoding(int blockSize, byte[][] encoding)
        {
            BlockSize = blockSize;
            Encoding = encoding;
        }

        public static unsafe ZtrFileEncoding ReadFromStream(Stream input)
        {
            int blockSize;
            byte[] buff = input.EnsureRead(4);
            fixed (byte* b = &buff[0])
                blockSize = Endian.ToBigInt32(b);

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

        public unsafe void WriteToStream(Stream output)
        {
            BinaryWriter bw = new BinaryWriter(output);
            bw.WriteBig(BlockSize);

            if (BlockSize > 0)
                throw new NotImplementedException();
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

        public unsafe static ZtrFileEncoding CompressZtrContent(byte[] data, ref int dataSize)
        {
            ushort maxValue = 0;
            int maxCounter = 0;
            int[] singleCounter = new int[byte.MaxValue + 1];
            int[] pairCounter = new int[ushort.MaxValue + 1];
            byte[,] encoding = new byte[256, 2];

            List<byte> unused = new List<byte>(256);
            fixed (byte* dataPtr = data)
            fixed (int* pairCounterPtr = pairCounter)
            fixed (int* singleCounterPtr = singleCounter)
            {
                int index;
                for (index = 0; index < 256; index++)
                {
                    bool finded = false;
                    for (int b = 0; b < dataSize; b++)
                    {
                        byte val = dataPtr[b];
                        if (index == val)
                        {
                            singleCounterPtr[val]++;
                            finded = true;
                        }
                    }
                    if (!finded)
                        unused.Add((byte)index);
                }

                for (int unusedIndex = 0; unusedIndex < unused.Count; unusedIndex++)
                {
                    byte value = unused[unusedIndex];
                    maxCounter = 0;

                    for (int i = 0; i < 256; i++)
                    {
                        int count = singleCounterPtr[i];
                        if (count > maxCounter)
                        {
                            index = i;
                            maxCounter = count;
                        }
                    }

                    if (maxCounter == 0)
                        break;

                    singleCounterPtr[index] = 0;
                    maxCounter = 0;

                    Array.Clear(pairCounter, 0, pairCounter.Length);

                    byte left, right;
                    for (int b = 0; b < dataSize - 1; b++)
                    {
                        left = dataPtr[b];
                        right = dataPtr[b + 1];
                        if (left != index && right != index)
                            continue;

                        ushort pair = (ushort)((left << 8) | right);
                        int count = ++pairCounterPtr[pair];
                        if (count > maxCounter)
                        {
                            maxCounter = count;
                            maxValue = pair;
                        }
                    }

                    if (maxCounter < 2)
                    {
                        unusedIndex--;
                        continue;
                    }

                    left = (byte)((maxValue >> 8) & 0xFF);
                    right = (byte)(maxValue & 0xFF);
                    encoding[value, 0] = left;
                    encoding[value, 1] = right;

                    for (int b = 0; b < dataSize - 1; b++)
                    {
                        if (dataPtr[b] != left || dataPtr[b + 1] != right)
                            continue;

                        dataPtr[b] = value;
                        for (int m = b + 1; m < dataSize - 1; m++)
                            dataPtr[m] = dataPtr[m + 1];

                        dataSize--;
                    }
                }
            }
            return null;
        }
    }
}