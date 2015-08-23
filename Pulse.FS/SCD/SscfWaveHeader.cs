using System;
using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public class SscfWaveHeader : IStreamingContent
    {
        public const int StructSize = 0x20;

        private long _inputOffset;

        public int DataLength { get; set; } // Does not include VoiceHeader or FormatHeader
        public int NumChannels { get; set; }
        public int SamplingRate { get; set; }
        public SscfWaveFormat Format { get; set; }
        public int NumSamples { get; set; }
        public int F { get; set; }
        public int FormatHeaderLength { get; set; }
        public int AuxChunkCount { get; set; }

        public long DataOffset
        {
            get { return _inputOffset + StructSize + FormatHeaderLength; }
        }

        public void ReadFromStream(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            _inputOffset = stream.Position;
            DataLength = br.ReadInt32();
            NumChannels = br.ReadInt32();
            SamplingRate = br.ReadInt32();
            Format = (SscfWaveFormat)br.ReadInt32();
            NumSamples = br.ReadInt32();
            F = br.ReadInt32();
            FormatHeaderLength = br.ReadInt32();
            AuxChunkCount = br.ReadInt32();

            if (AuxChunkCount != 0)
                throw new NotImplementedException();
        }

        public void WriteToStream(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);
            bw.Write(DataLength);
            bw.Write(NumChannels);
            bw.Write(SamplingRate);
            bw.Write((int)Format);
            bw.Write(NumSamples);
            bw.Write(F);
            bw.Write(FormatHeaderLength);
            bw.Write(AuxChunkCount);
        }
    }
}