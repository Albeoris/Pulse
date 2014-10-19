using System.Runtime.InteropServices;

namespace Pulse.FS
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ArchiveListingHeader
    {
        public int BlockOffset;
        public int InfoOffset;
        public int EntriesCount;

        public int BlocksCount
        {
            get { return (InfoOffset - BlockOffset) / 12; }
        }
    }
}