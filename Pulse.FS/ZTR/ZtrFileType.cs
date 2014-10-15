namespace Pulse.FS
{
    public enum ZtrFileType
    {
        LittleEndianCompressedDictionary = 0,
        BigEndianUncompressedPair = 1,
        
        BigEndianUncompressedDictionary // > 1
    }
}