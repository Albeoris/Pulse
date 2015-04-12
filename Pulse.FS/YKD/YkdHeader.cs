using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class YkdHeader : IStreamingContent
    {
        public const int Size = 16;
        public const int MagicNumber = 0x5F444B59;

        public int Magic;
        public int Unknown1;
        public byte Unknown2, Unknown3, Unknown4, Unknown5;
        public int Dummy; // Always 0

        public void ReadFromStream(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            Magic = br.Check(reader => reader.ReadInt32(), MagicNumber);
            Unknown1 = br.ReadInt32();
            Unknown2 = br.ReadByte();
            Unknown3 = br.ReadByte();
            Unknown4 = br.ReadByte();
            Unknown5 = br.ReadByte();
            Dummy = br.Check(reader => reader.ReadInt32(), 0);
        }

        public void WriteToStream(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);
            bw.Write(Magic);
            bw.Write(Unknown1);
            bw.Write(Unknown2);
            bw.Write(Unknown3);
            bw.Write(Unknown4);
            bw.Write(Unknown5);
            bw.Write(Dummy);
        }
    }
}