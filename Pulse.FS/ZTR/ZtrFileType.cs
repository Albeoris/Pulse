namespace Pulse.FS
{
    public enum ZtrFileType
    {
        BigEndianCompressedDictionary = 0,
        LittleEndianUncompressedPair = 1,

        LittleEndianUncompressedDictionary // > 1
    }
}