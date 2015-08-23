using System;
using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class SeDbResEntry : IStreamingContent
    {
        public int Index;
        public int Offset;
        public int Length;
        public int Unknown;

        public void ReadFromStream(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);

            Index = br.ReadInt32();
            Offset = br.ReadInt32();
            Length = br.ReadInt32();
            Unknown = br.ReadInt32();
        }

        public void WriteToStream(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}