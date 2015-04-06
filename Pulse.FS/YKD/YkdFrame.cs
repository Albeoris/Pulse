using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class YkdFrame : IStreamingContent
    {
        public const int Size = 32;

        public int Unknown1;
        public int Unknown2;
        public int Unknown3;
        public int Unknown4;
        public int Unknown5;
        public int Unknown6;
        public int X;
        public int Y;

        public void ReadFromStream(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);

            Unknown1 = br.ReadInt32();
            Unknown2 = br.ReadInt32();
            Unknown3 = br.ReadInt32();
            Unknown4 = br.ReadInt32();
            Unknown5 = br.ReadInt32();
            Unknown6 = br.ReadInt32();
            X = br.ReadInt32();
            Y = br.ReadInt32();
        }

        public void WriteToStream(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);

            bw.Write(Unknown1);
            bw.Write(Unknown2);
            bw.Write(Unknown3);
            bw.Write(Unknown4);
            bw.Write(Unknown5);
            bw.Write(Unknown6);
            bw.Write(X);
            bw.Write(Y);
        }
    }
}