using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class AdPcmWaveExtraData : IStreamingContent
    {
        public short SamplesPerBlock;
        public short CoefficientsCount;
        public AdPcmCoefficientSet[] Coefficients;

        public void ReadFromStream(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            SamplesPerBlock = br.ReadInt16();
            CoefficientsCount = br.ReadInt16();
            Coefficients = stream.ReadContent<AdPcmCoefficientSet>(CoefficientsCount);
        }

        public void WriteToStream(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);
            bw.Write(SamplesPerBlock);
            bw.Write(CoefficientsCount);
            stream.WriteContent(Coefficients);
        }
    }
}