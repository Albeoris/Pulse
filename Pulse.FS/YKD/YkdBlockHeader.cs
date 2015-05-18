using System;
using System.IO;
using System.Linq;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class YkdBlock : IStreamingContent
    {
        public const int TransformationMatrixSize = 0x50;

        public bool IsFirstBlock;
        public uint Type, Index, AssociatedIndex, Unknown;
        public readonly int[] TransformationMatrix = new int[TransformationMatrixSize / 4];

        public YkdOffsets Offsets;
        public YkdBlockEntry[] Entries;
        public YkdBlockOptionalTail ZeroTail;
        public YkdBlockOptionalTails Tails4;
        public int[] Tail56;

        public int CalcSize()
        {
            YkdBlockEntry[] entries = Entries ?? new YkdBlockEntry[0];
            YkdOffsets offsets = new YkdOffsets {Offsets = new int[entries.Length]};

            int result = 4 * 4 + TransformationMatrixSize + offsets.CalcSize() + entries.Sum(t => t.CalcSize());

            if (ZeroTail != null)
                result += YkdBlockOptionalTail.Size;
            else if (Tails4 != null)
                result += Tails4.CalcSize();
            else if (Tail56 != null)
                result += Tail56.Length * 4;

            return result;
        }

        public void ReadFromStream(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);

            IsFirstBlock = stream.Position == YkdHeader.Size;

            Type = br.ReadUInt32();
            Index = br.ReadUInt32();
            AssociatedIndex = br.ReadUInt32();
            Unknown = br.ReadUInt32();

            for (int i = 0; i < TransformationMatrix.Length; i++)
                TransformationMatrix[i] = br.ReadBigInt32();

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
                        Tail56 = new int[12];
                        for (int i = 0; i < Tail56.Length; i++)
                            Tail56[i] = br.ReadInt32();
                        break;
                }
            }
        }

        public void WriteToStream(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);

            bw.Write(Type);
            bw.Write(Index);
            bw.Write(AssociatedIndex);
            bw.Write(Unknown);

            for (int i = 0; i < TransformationMatrix.Length; i++)
                bw.WriteBig(TransformationMatrix[i]);

            YkdOffsets.WriteToStream(stream, ref Offsets, ref Entries, b => b.CalcSize());
            stream.WriteContent(Entries);

            if (ZeroTail != null)
                stream.WriteContent(ZeroTail);
            else if (Tails4 != null)
                stream.WriteContent(Tails4);
            else if (Tail56 != null)
                for (int i = 0; i < Tail56.Length; i++)
                    bw.Write(Tail56[i]);
        }
    }
}