using System;
using System.IO;
using System.Linq;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class YkdBlock : IStreamingContent
    {
        public const int DataSize = 0x50;

        public bool IsFirstBlock;
        public uint Type, Unknown2, ParentMaybe, Unknown4;
        public readonly int[] Data = new int[DataSize / 4];

        public YkdOffsets Offsets;
        public YkdBlockEntry[] Entries;
        public YkdBlockOptionalTail ZeroTail;
        public YkdBlockOptionalTails Tails4;
        public byte[] Tail56;

        public int CalcSize()
        {
            YkdBlockEntry[] entries = Entries ?? new YkdBlockEntry[0];
            YkdOffsets offsets = new YkdOffsets {Offsets = new int[entries.Length]};

            int result = 4 * 4 + DataSize + offsets.CalcSize() + entries.Sum(t => t.CalcSize());
            
            if (ZeroTail != null)
                result += YkdBlockOptionalTail.Size;
            else if (Tails4 != null)
                result += Tails4.CalcSize();
            else if (Tail56 != null)
                result += Tail56.Length;

            return result;
        }

        public void ReadFromStream(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);

            IsFirstBlock = stream.Position == YkdHeader.Size;

            Type = br.ReadUInt32();
            Unknown2 = br.ReadUInt32();
            ParentMaybe = br.ReadUInt32();
            Unknown4 = br.ReadUInt32();

            for (int i = 0; i < Data.Length; i++)
                Data[i] = br.ReadInt32();

            Offsets = stream.ReadContent<YkdOffsets>();
            Entries = new YkdBlockEntry[Offsets.Count];
            for (int i = 0; i < Offsets.Count; i++)
            {
                stream.SetPosition(Offsets[i]);
                Entries[i] = stream.ReadContent<YkdBlockEntry>();
            }

            if (!IsFirstBlock)
            {
                switch (Type)
                {
                    case 0:
                        ZeroTail = stream.ReadContent<YkdBlockOptionalTail>();
                        break;
                    case 4:
                        Tails4 = stream.ReadContent<YkdBlockOptionalTails>();
                        break;
                    case 5:
                    case 6:
                        Tail56 = stream.EnsureRead(48);
                        break;
                }
            }
        }

        public void WriteToStream(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);

            bw.Write(Type);
            bw.Write(Unknown2);
            bw.Write(ParentMaybe);
            bw.Write(Unknown4);

            for (int i = 0; i < Data.Length; i++)
                bw.Write(Data[i]);
            
            YkdOffsets.WriteToStream(stream, ref Offsets, ref Entries, b => b.CalcSize());
            stream.WriteContent(Entries);

            if (ZeroTail != null)
                stream.WriteContent(ZeroTail);
            else if (Tails4 != null)
                stream.WriteContent(Tails4);
            else if (Tail56 != null)
                stream.Write(Tail56, 0, Tail56.Length);
        }
    }
}