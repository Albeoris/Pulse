using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class YkdBlockOptionalTails : IStreamingContent
    {
        public int Count;

        public int Unknown1;
        public int Unknown2;
        public int Unknown3;

        public YkdBlockOptionalTail[] Tails;

        public YkdBlockOptionalTail this[int index]
        {
            get { return Tails[index]; }
            set { Tails[index] = value; }
        }

        public int CalcSize()
        {
            return 16 + Count * YkdBlockOptionalTail.Size;
        }

        public void ReadFromStream(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            Count = br.ReadInt32();
            Unknown1 = br.ReadInt32();
            Unknown2 = br.ReadInt32();
            Unknown3 = br.ReadInt32();
            Tails = stream.ReadContent<YkdBlockOptionalTail>(Count);
        }

        public void WriteToStream(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);
            bw.Write(Count);
            bw.Write(Unknown1);
            bw.Write(Unknown2);
            bw.Write(Unknown3);
            stream.WriteContent(Tails);
        }
    }
}