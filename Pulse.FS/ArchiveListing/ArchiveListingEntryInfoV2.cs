using System.Runtime.InteropServices;

namespace Pulse.FS
{
    /// <summary>
    /// Borrowed: http://forum.xentax.com/viewtopic.php?f=10&t=10985&start=15
    /// Thank you rhadamants
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ArchiveListingEntryInfoV2
    {
        public short UnknownValue;
        public short UnknownNumber;
        public short Offset; // Exact position in file chunk with info about this file. Offsets are funky and doesn't always reset to 0 for each file chunk.
        public short Unknown3; // Supposed to be fileChunk? Don't know how it works.
    }
}