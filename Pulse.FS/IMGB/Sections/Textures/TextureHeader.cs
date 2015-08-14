using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    /// <summary>Header for SEDBtxb sections</summary>
    public struct TextureHeader : IStreamingContent
    {
        public int Unknown1;
        public int Unknown2;
        public int Unknown3;
        public int Unknown4;

        public void ReadFromStream(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            Unknown1 = br.ReadBigInt32();
            Unknown2 = br.ReadBigInt32();
            Unknown3 = br.ReadBigInt32();
            Unknown4 = br.ReadBigInt32();
        }

        public void WriteToStream(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);
            bw.WriteBig(Unknown1);
            bw.WriteBig(Unknown2);
            bw.WriteBig(Unknown3);
            bw.WriteBig(Unknown4);
        }
    }
}