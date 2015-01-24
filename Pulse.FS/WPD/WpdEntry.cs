namespace Pulse.FS
{
    public sealed class WpdEntry : IArchiveEntry
    {
        public string Name { get; private set; }
        public string Extension;
        public int Offset;
        public int Length;

        public WpdEntry(string name, int offset, int length, string extension)
        {
            Name = name;
            Offset = offset;
            Length = length;
            Extension = extension;
        }
    }
}