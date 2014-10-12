namespace Pulse.FS
{
    public sealed class ArchiveListingEntry
    {
        private const int SectorSize = 0x800;

        public readonly string Name;
        public readonly long Sector;
        public readonly long Size;
        public readonly long UncompressedSize;

        public ArchiveListingEntry(string name, long sector, long size, long uncompressedSize)
        {
            Name = name;
            Sector = sector;
            Size = size;
            UncompressedSize = uncompressedSize;
        }

        public long Offset
        {
            get { return Sector * SectorSize; }
        }
    }
}