namespace Pulse.FS
{
    public struct ZtrFileHeaderLineInfo
    {
        public byte Block;
        public byte BlockOffset;
        public ushort PackedOffset;
        
        public int UnpackedOffset;
        public int UnpackedLength;
    }
}