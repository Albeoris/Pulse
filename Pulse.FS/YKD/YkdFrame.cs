using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class YkdFrame : IStreamingContent
    {
        public const int Size = 32;

        public int Unknown1;
        public float Unknown2;
        public int Unknown3;
        public int Unknown4;
        public float Unknown5;
        public float Unknown6;
        public float X;
        public float Y;

        public void ReadFromStream(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);

            Unknown1 = br.ReadInt32();
            Unknown2 = br.ReadSingle();
            Unknown3 = br.ReadInt32();
            Unknown4 = br.ReadInt32();
            Unknown5 = br.ReadSingle();
            Unknown6 = br.ReadSingle();
            X = br.ReadSingle();
            Y = br.ReadSingle();
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