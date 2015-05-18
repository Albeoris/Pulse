using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public class SscfHeader : IStreamingContent
    {
        public short Unknown0; // 0x1
        public short NumTracks;
        public short NumWaves;
        public short Unknown1; // 0x100F, 0x1389, 0x138A  -- possible flags, format?
        public int OffsetA;
        public int WavesOffset; // offset to list of wave offsets
        public long OffsetC;
        public long OffsetD;
        public long OffsetE;
        public long Unused; // 0x0

        public void ReadFromStream(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            Unknown0 = br.ReadInt16();
            NumTracks = br.ReadInt16();
            NumWaves = br.ReadInt16();
            Unknown1 = br.ReadInt16();
            OffsetA = br.ReadInt32();
            WavesOffset = br.ReadInt32();
            OffsetC = br.ReadInt64();
            OffsetD = br.ReadInt64();
            OffsetE = br.ReadInt64();
            Unused = br.ReadInt64();
        }

        public void WriteToStream(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);
            bw.Write(Unknown0);
            bw.Write(NumTracks);
            bw.Write(NumWaves);
            bw.Write(Unknown1);
            bw.Write(OffsetA);
            bw.Write(WavesOffset);
            bw.Write(OffsetC);
            bw.Write(OffsetD);
            bw.Write(OffsetE);
            bw.Write(Unused);
        }
    }
}