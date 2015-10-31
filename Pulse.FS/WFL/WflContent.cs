namespace Pulse.FS
{
    public sealed class WflContent
    {
        public const int CharactersCount = 224;
        public const int AdditionalTableCount = 0x2C10;

        public readonly WflHeader Header;
        public readonly int[] Sizes;
        public readonly int[] Offsets;
        public readonly int[] Colors; // Unknown
        public readonly short[] AdditionalTable;

        public WflContent(WflHeader header, int[] sizes, int[] offsets, int[] colors, short[] additionalTable)
        {
            Header = header;
            Sizes = sizes;
            Offsets = offsets;
            Colors = colors;
            AdditionalTable = additionalTable;
        }

        public void GetOffsets(int index, out int x, out int y)
        {
            int offsets = Offsets[index];
            x = offsets & 0xFFFF;
            y = (offsets >> 16) & 0xFFFF;
        }

        public void SetOffsets(int index, int x, int y)
        {
            Offsets[index] = x | (y << 16);
        }

        public void GetSizes(int index, out byte before, out byte width, out byte after)
        {
            int sizes = Sizes[index];
            before = (byte)(sizes & 0xFF);
            width = (byte)((sizes >> 8) & 0xFF);
            after = (byte)((sizes >> 16) & 0xFF);
        }

        public void SetSizes(int index, byte before, byte width, byte after)
        {
            Sizes[index] = before | (width << 8) | (after << 16);
        }
    }
}