using System.Runtime.InteropServices;

namespace Pulse.FS
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ArchiveListingHeaderV2 : IArchiveListingHeader
    {
        private const int KeyDataSize = 32;

        public unsafe fixed byte KeyData [32];
        public int RawBlockOffset;
        public int RawInfoOffset;
        public int EntriesCount { get; set; }

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
    }
}