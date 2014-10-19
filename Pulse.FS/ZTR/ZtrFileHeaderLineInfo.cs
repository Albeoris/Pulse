namespace Pulse.FS
{
    public struct ZtrFileHeaderLineInfo
    {
        private const int SectorSize = 0x800;

        public byte Block;
        public byte BlockOffset;
        public ushort PackedOffset;
        
        public int UnpackedOffset;
        public int UnpackedLength;
    }
}