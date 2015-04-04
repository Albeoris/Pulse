using System.IO;
using Pulse.Core;

namespace Pulse.FS.YKD
{
    public sealed class YkdHeader : IStreamingContent
    {
        public const int MagicNumber = 0x5F444B59;

        public int Magic;
        private int Unknown1;
        private byte Unknown2, Unknown3, Unknown4, Unknown5;
        private int Dummy;

        public void ReadFromStream(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            Magic = br.ReadInt32();
            if (Magic != MagicNumber)
                throw new InvalidDataException(Magic.ToString());
            Unknown1 = br.ReadInt32();
            Unknown2 = br.ReadByte();
            Unknown3 = br.ReadByte();
            Unknown4 = br.ReadByte();
            Unknown5 = br.ReadByte();
            Dummy = br.ReadInt32();
        }

        public void WriteToStream(Stream stream)
        {
            throw new System.NotImplementedException();
        }
    }
}