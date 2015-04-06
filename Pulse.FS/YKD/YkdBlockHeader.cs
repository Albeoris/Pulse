using System;
using System.IO;
using System.Linq;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class YkdBlock : IStreamingContent
    {
        public uint Unknown1, Unknown2, Unknown3, Unknown4;
        public readonly byte[] Data = new byte[0x60];

        public YkdOffsets Offsets;
        public YkdBlockEntry[] Entries;

        public int CalcSize()
        {
            YkdBlockEntry[] entries = Entries ?? new YkdBlockEntry[0];
            YkdOffsets offsets = new YkdOffsets {Offsets = new int[entries.Length]};

            return 4 * 4 + 0x60 + offsets.CalcSize() + entries.Sum(t => t.CalcSize());
        }

        public void ReadFromStream(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);

            Unknown1 = br.ReadUInt32();
            Unknown2 = br.ReadUInt32();
            Unknown3 = br.ReadUInt32();
            Unknown4 = br.ReadUInt32();

            stream.EnsureRead(Data, 0, Data.Length);

            Offsets = stream.ReadContent<YkdOffsets>();
            Entries = new YkdBlockEntry[Offsets.Count];
            for (int i = 0; i < Offsets.Count; i++)
            {
                stream.SetPosition(Offsets[i]);
                Entries[i] = stream.ReadContent<YkdBlockEntry>();
            }
        }

        public void WriteToStream(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);

            bw.Write(Unknown1);
            bw.Write(Unknown2);
            bw.Write(Unknown3);
            bw.Write(Unknown4);

            stream.Write(Data, 0, Data.Length);
            YkdOffsets.WriteToStream(stream, ref Offsets, ref Entries, b => b.CalcSize());
            stream.WriteContent(Entries);
        }
    }
}