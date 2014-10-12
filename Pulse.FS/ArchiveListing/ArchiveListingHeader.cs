namespace Pulse.FS
{
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