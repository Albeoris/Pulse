using System;
using System.IO;
using System.Text;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class SeDbResHeader : IStreamingContent
    {
        public const long MagicNumber = 0x2053455242444553; // "SEDBRES "
        public const int UnknownValue1 = 0x00000FA0;
        public const int UnknownValue2 = 0x00400200;

        public long Magic = MagicNumber;
        public int Unknown1 = UnknownValue1;
        public int Unknown2 = UnknownValue2;
        public byte[] UnknownBuff = new byte[32];

        public int Count;
        public int Unknown3;
        public int Unknown4;
        public int Unknown5;

        public SeDbResEntry[] Entries;

        public void ReadFromStream(Stream input)
        {
            BinaryReader br = new BinaryReader(input);
            
            Magic = br.ReadInt64();
            Unknown1 = br.ReadInt32();
            Unknown2 = br.ReadInt32();
            
            br.Read(UnknownBuff, 0, UnknownBuff.Length);
            
            Count = br.ReadInt32();
            Unknown3 = br.ReadInt32();
            Unknown4 = br.ReadInt32();
            Unknown5 = br.ReadInt32();

            Entries = input.ReadContent<SeDbResEntry>(Count);
        }

        public void WriteToStream(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}