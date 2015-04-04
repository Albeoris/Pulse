using System;
using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class SectionHeader : IStreamingContent
    {
        public const uint MagicNumber = 0x42444553;

        public int Magic;
        public SectionType Type;
        public int Version;
        public int Unknown2;
        public int SectionLength; // Включая размер заголовков (>= 48)
        public byte[] Junk;

        public void ReadFromStream(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            Magic = br.ReadInt32();
            if (Magic != MagicNumber)
                throw new Exception("Неверная сигнатура файла: " + Magic);

            Type = (SectionType)br.ReadInt32();
            Version = br.ReadBigInt32();
            Unknown2 = br.ReadBigInt32();
            SectionLength = br.ReadBigInt32();
            Junk = stream.EnsureRead(28);
        }

        public void WriteToStream(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);
            bw.Write(Magic);
            bw.Write((int)Type);
            bw.WriteBig(Version);
            bw.WriteBig(Unknown2);
            bw.WriteBig(SectionLength);
            stream.Write(Junk, 0, Junk.Length);
        }
    }
}