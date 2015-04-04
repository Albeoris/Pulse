namespace Pulse.FS
{
    public sealed class WpdEntry : IArchiveEntry
    {
        public int Index { get; private set; }
        public string Name { get; private set; }
        public string Extension;
        public int Offset;
        public int Length;

        public WpdEntry(int index, string name, int offset, int length, string extension)
        {
            Index = index;
            Name = name;
            Offset = offset;
            Length = length;
            Extension = extension;
        }

        public string GetNameWithExtension()
        {
            return Name + '.' + Extension;
        }
    }
}