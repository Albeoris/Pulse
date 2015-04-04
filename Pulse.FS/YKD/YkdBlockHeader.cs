using System.IO;
using Pulse.Core;

namespace Pulse.FS.YKD
{
    public sealed class YkdBlockHeader : IStreamingContent
    {
        private uint Unknown1, Unknown2, Unknown3, Unknown4;
        private readonly byte[] Data = new byte[0x60];
        private int[] Offsets;
        private uint Count1, Count2, Count3, Count4;

        public void ReadFromStream(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);

            Unknown1 = br.ReadUInt32();
            Unknown2 = br.ReadUInt32();
            Unknown3 = br.ReadUInt32();
            Unknown4 = br.ReadUInt32();

            stream.EnsureRead(Data, 0, Data.Length);


            Offsets = new int[Count1];
            for (int i = 0; i < Count1; i++)
                Offsets[i] = br.ReadInt32();
            stream.Seek(((4 - (Count1 % 4))%4) * 4, SeekOrigin.Current);
        }

        public void WriteToStream(Stream stream)
        {
            throw new System.NotImplementedException();
        }
    }
}