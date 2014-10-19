using System.Runtime.InteropServices;

namespace Pulse.FS
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ArchiveListingBlockInfo
    {
        public int UncompressedSize;
        public int CompressedSize;
        public int Offset;
    }
}