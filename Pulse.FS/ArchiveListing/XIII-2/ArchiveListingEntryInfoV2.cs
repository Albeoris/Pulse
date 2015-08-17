using System;
using System.IO;
using System.Runtime.InteropServices;
using Pulse.Core;

namespace Pulse.FS
{
    /// <summary>
    /// Borrowed: http://forum.xentax.com/viewtopic.php?f=10&t=10985&start=15
    /// Thank you rhadamants
    /// </summary>
    public sealed class ArchiveListingEntryInfoV2 : IStreamingContent
    {
        public short UnknownValue;
        public short UnknownNumber;
        public short RawOffset; // Exact position in file chunk with info about this file. Offsets are funky and doesn't always reset to 0 for each file chunk.
        public short UnknownData; // Supposed to be fileChunk? Don't know how it works.

        public short BlockNumber;

        public Boolean Flag
        {
            get { return (RawOffset & 0x8000) == 0x8000; }
            set
            {
                if (value)
                    RawOffset = (short)(RawOffset | 0x8000);
                else
                    RawOffset = (short)(RawOffset & ~0x8000);
            }
        }

        public short Offset
        {
            get { return (short)(RawOffset & 0x7FFF); }
            set { RawOffset = (short)((value & 0x7FFF) | (RawOffset & 0x8000)); }
        }

        public void ReadFromStream(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);

            UnknownValue = br.ReadInt16();
            UnknownNumber = br.ReadInt16();
            RawOffset = br.ReadInt16();
            UnknownData = br.ReadInt16();
        }

        public void WriteToStream(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);
            bw.Write(UnknownValue);
            bw.Write(UnknownNumber);
            bw.Write(RawOffset);
            bw.Write(UnknownData);
        }
    }
}