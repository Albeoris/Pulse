namespace Pulse.FS
{
    public sealed class ArchiveEntry : IArchiveEntry
    {
        private const int SectorSize = 0x800;

        public string Name { get; private set; }
        public long Sector;
        public long Size;
        public long UncompressedSize;
        public short UnknownNumber, UnknownValue, UnknownData;

        public ArchiveEntry(string name, long sector, long size, long uncompressedSize)
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

        public bool IsCompressed
        {
            get { return Size > 0 && Size != UncompressedSize; }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}