namespace Pulse.FS
{
    public sealed class WpdEntry : IArchiveEntry
    {
        public int Index { get; private set; }
        public string NameWithoutExtension { get; }
        public readonly string Extension;
        public int Offset;
        public int Length;

        public WpdEntry(int index, string name, int offset, int length, string extension)
        {
            Index = index;
            NameWithoutExtension = name;
            Offset = offset;
            Length = length;
            Extension = extension;
        }

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(Extension))
                    return NameWithoutExtension;

                return NameWithoutExtension + '.' + Extension;
            }
        }
    }
}