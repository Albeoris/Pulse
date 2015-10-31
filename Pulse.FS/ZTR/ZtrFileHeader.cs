using System;
using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ZtrFileHeader
    {
        public int Version;
        public int Count;
        public int KeysUnpackedSize;
        public int TextBlocksCount;
        public int[] TextBlockTable;
        public ZtrFileHeaderLineInfo[] TextLinesTable;

        public unsafe void ReadFromStream(Stream input)
        {
            byte[] buff = input.EnsureRead(0x10);
            fixed (byte* b = &buff[0])
            {
                Version = Endian.ToBigInt32(b + 0);
                Count = Endian.ToBigInt32(b + 4);
                KeysUnpackedSize = Endian.ToBigInt32(b + 8);
                TextBlocksCount = Endian.ToBigInt32(b + 12);
            }

            if (Version != 1)
                throw new NotImplementedException();

            TextBlockTable = new int[TextBlocksCount];
            if (TextBlocksCount > 0)
            {
                buff = input.EnsureRead(TextBlocksCount * 4);
                fixed (byte* b = &buff[0])
                {
                    for (int i = 0; i < TextBlocksCount; i++)
                        TextBlockTable[i] = Endian.ToBigInt32(b + i * 4);
                }
            }

            TextLinesTable = new ZtrFileHeaderLineInfo[Count];
            if (Count > 0)
            {
                buff = input.EnsureRead(Count * 4);
                fixed (byte* b = &buff[0])
                {
                    for (int i = 0; i < Count; i++)
                    {
                        TextLinesTable[i].Block = *(b + i * 4);
                        TextLinesTable[i].BlockOffset = *(b + i * 4 + 1);
                        TextLinesTable[i].PackedOffset = Endian.ToBigUInt16(b + i * 4 + 2);
                    }
                }
            }
        }

        public void WriteToStream(Stream output)
        {
            if (Version != 1)
                throw new NotImplementedException();
            if (TextBlockTable.Length != TextBlocksCount)
                throw new InvalidDataException();
            if (TextLinesTable.Length != Count)
                throw new InvalidDataException();

            BinaryWriter bw = new BinaryWriter(output);
            bw.WriteBig(Version);
            bw.WriteBig(Count);
            bw.WriteBig(KeysUnpackedSize);
            bw.WriteBig(TextBlocksCount);

            foreach (int value in TextBlockTable)
                bw.WriteBig(value);

            foreach (ZtrFileHeaderLineInfo value in TextLinesTable)
            {
                bw.Write(value.Block);
                bw.Write(value.BlockOffset);
                bw.WriteBig(value.PackedOffset);
            }
        }
    }
}