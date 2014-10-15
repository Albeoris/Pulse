using System;
using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ZtrFileHeader
    {
        public int Version;
        public int TagsTableCount;
        public int TagsUnpackedSize;
        public int TextTableCount;
        public int[] TextBlockTable;
        public int[] TextLinesTable;

        public unsafe void ReadFromStream(Stream input)
        {
            byte[] buff = input.EnsureRead(0x10);
            fixed (byte* b = &buff[0])
            {
                Version = Endian.ToLittleInt32(b + 0);
                TagsTableCount = Endian.ToLittleInt32(b + 4);
                TagsUnpackedSize = Endian.ToLittleInt32(b + 8);
                TextTableCount = Endian.ToLittleInt32(b + 12);
            }

            if (Version != 1)
                throw new NotImplementedException();

            TextBlockTable = new int[TextTableCount];
            buff = input.EnsureRead(TextTableCount * 4);
            fixed (byte* b = &buff[0])
            {
                for (int i = 0; i < TextTableCount; i++)
                    TextBlockTable[i] = Endian.ToLittleInt32(b + i * 4);
            }

            TextLinesTable = new int[TagsTableCount];
            buff = input.EnsureRead(TagsTableCount * 4);
            fixed (byte* b = &buff[0])
            {
                for (int i = 0; i < TagsTableCount; i++)
                    TextLinesTable[i] = Endian.ToLittleInt32(b + i * 4);
            }
        }
    }
}