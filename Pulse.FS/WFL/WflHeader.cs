using System.Runtime.InteropServices;

namespace Pulse.FS
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public sealed class WflHeader
    {
        public const int SmallTable = 0x14;
        public const int LargeTable = 0x1E;

        public const int ColorTable40C = 0x12;
        public const int ColorTable460 = 0x13;
        public const int ColorTable530 = 0x1E;
        public const int ColorTableFF23 = 0x20;

        public int TableType; // ?
        public int LineSpacing;
        public int LineHeight;
        public int SquareDiff; // LineSpacing + SquareDiff = Размера квадратных символов
        public int Offset;
        public int ColorTableType;
        public int Unknown02;
        public int Unknown03;

        public int SquareSize
        {
            get { return LineSpacing + SquareDiff; }
        }
    }
}