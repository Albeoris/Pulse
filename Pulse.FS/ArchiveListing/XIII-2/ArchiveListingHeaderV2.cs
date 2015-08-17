using System;
using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ArchiveListingHeaderV2 : IArchiveListingHeader, IStreamingContent
    {
        private const int KeyDataSize = 32;

        // Streaming Data
        public readonly Byte[] KeyData = new byte[KeyDataSize];
        public Int32 RawBlockOffset;
        public Int32 RawInfoOffset;
        public Int32 EntriesCount { get; set; }

        public bool IsEncrypted;
        public int BlockOffset => RawBlockOffset + KeyDataSize;
        public int InfoOffset => RawInfoOffset + KeyDataSize;
        public int BlocksCount => (RawInfoOffset - RawBlockOffset) / 12;

        public bool IsValid(long fileSize)
        {
            if (RawBlockOffset > RawInfoOffset || RawBlockOffset < 0 || RawInfoOffset < 0)
                return false;

            if (EntriesCount < 0)
                return false;

            if (BlocksCount > short.MaxValue)
                return false;

            if (BlockOffset > fileSize || InfoOffset > fileSize)
                return false;

            return true;
        }

        public void ReadFromStream(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            stream.EnsureRead(KeyData, 0, KeyDataSize);

            RawBlockOffset = br.ReadInt32();
            RawInfoOffset = br.ReadInt32();
            EntriesCount = br.ReadInt32();
        }

        public void WriteToStream(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);
            stream.Write(KeyData, 0, KeyDataSize);

            bw.Write(RawBlockOffset);
            bw.Write(RawInfoOffset);
            bw.Write(EntriesCount);
        }
    }
}