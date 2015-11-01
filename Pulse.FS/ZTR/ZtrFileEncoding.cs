using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
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

        public static byte[] CompressZtrContent([In, Out] byte[] data, int dataIndex, ref ushort dataSize)
        {
            return CompressZtrContent(data, dataIndex, ref dataSize, new ushort[0,0], 0, 0);
        }

        // WARNING BUG: Don't use it for text packing or fix BlockOffset first!
        // It should be contains offset if the begining of the entry was mixed with a previous tail.
        public static unsafe byte[] CompressZtrContent([In, Out] byte[] data, int dataIndex, ref ushort dataSize, [In, Out] ushort[,] innerOffsets, int innerIndex, int innerCount)
        {
            ushort maxValue = 0;
            int[] singleCounter = new int[byte.MaxValue + 1];
            int[] pairCounter = new int[ushort.MaxValue + 1];
            byte[,] encoding = new byte[256, 2];

            List<byte> unused = new List<byte>(256);
            List<byte> used = new List<byte>(256);
            fixed (byte* dataPtr = &data[dataIndex])
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
                    int maxCounter = 0;

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
                    used.Add(value);

                    for (int b = 0; b < dataSize - 1; b++)
                    {
                        if (dataPtr[b] != left || dataPtr[b + 1] != right)
                            continue;

                        // BlockOffset, (bug -_-)!
                        for (int i = 0; i < innerCount - 1; i++)
                        {
                            ushort offset = innerOffsets[i + innerIndex, 0];
                            if (offset > b + 1)
                                break;

                            if (offset == b + 1)
                            {
                                innerOffsets[i + innerIndex + 1, 1]++;
                                break;
                            }
                        }

                        dataPtr[b] = value;

                        for (int m = b + 1; m < dataSize - 1; m++)
                            dataPtr[m] = dataPtr[m + 1];

                        // PackedOffset
                        for (int i = 0; i < innerCount; i++)
                        {
                            if (innerOffsets[i + innerIndex, 0] > b)
                                innerOffsets[i + innerIndex, 0]--;
                        }

                        dataSize--;
                    }
                }
            }

            int blockSize = used.Count * 3;
            byte[] result = new byte[blockSize + 4];
            fixed (byte* arrayPtr = &result[0])
            {
                BinaryWriterExm.WriteBig(arrayPtr, blockSize);
                byte* ptr = arrayPtr + 4;

                for (int index = 0; index < used.Count; index++)
                {
                    byte value = used[index];
                    ptr[0] = value;
                    ptr[1] = encoding[value, 0];
                    ptr[2] = encoding[value, 1];
                    ptr += 3;
                }
            }

            return result;
        }

        public static unsafe byte[] FakeCompression([In, Out] byte[] data, int dataIndex, ref ushort dataSize, [In, Out] ushort[,] innerOffsets, int innerIndex, int innerCount)
        {
            const int blockSize = 0;
            byte[] result = new byte[blockSize + 4];
            fixed (byte* arrayPtr = &result[0])
                BinaryWriterExm.WriteBig(arrayPtr, blockSize);

            return result;
        }
    }
}