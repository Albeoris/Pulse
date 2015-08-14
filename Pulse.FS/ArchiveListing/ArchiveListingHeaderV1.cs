using System;
using System.Runtime.InteropServices;

namespace Pulse.FS
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ArchiveListingHeaderV1 : IArchiveListingHeader
    {
        public int BlockOffset { get; set; }
        public int InfoOffset { get; set; }
        public int EntriesCount { get; set; }

        public int BlocksCount
        {
            get { return (InfoOffset - BlockOffset) / 12; }
        }
    }
}