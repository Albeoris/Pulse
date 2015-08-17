using System.Runtime.InteropServices;

namespace Pulse.FS
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ArchiveListingEntryInfoV1
    {
        public short UnknownValue;
        public short UnknownNumber;
        public short BlockNumber;
        public short Offset;
    }
}