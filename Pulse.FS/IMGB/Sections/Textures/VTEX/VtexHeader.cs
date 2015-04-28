using System;
using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    /// <summary>
    /// See also:
    /// https://github.com/nohbdy/ffxivmodelviewer/blob/master/src/DatDigger/Sections/Texture/VtexSection.cs
    /// </summary>
    public class VtexHeader : IStreamingContent
    {
        public const int Size = 0x64;

        private const int NameSize = 16;
        private const int ExtensionSize = 8;

        public int Unknown1; //* 00 00 00 00
        public int DataLength; //* B4 40 00 00 
        public short Unknown2; //* 01 00 
        public short Unknown3; //* 01 00 
        public short Unknown4; //* 02 00

        public short UnknownCounterOrType; //* 02 00 // Какой-то счётчик или тип

        public int[] Unknown6;

        public string Name; //* 30 45 30 49 31 77 68 69 74 30 5F 66 30 32 74 00
        public string Extension; //* 78 65 74 76 00 00 00 00
        public int Unknown17; //* 01 00 00 00
        public int GtexOffset; // 64 00 00 00
        public int Unknown18; // 20 40 00 00
        public int Unknown19; // 00 00 00 00

        public void ReadFromStream(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            Unknown1 = br.ReadInt32();
            DataLength = br.ReadInt32();
            Unknown2 = br.ReadInt16();
            Unknown3 = br.ReadInt16();
            Unknown4 = br.ReadInt16();
            UnknownCounterOrType = br.ReadInt16();

            int count = 0;
            switch (UnknownCounterOrType)
            {
                case 0:
                    count = 8;
                    break;
                case 2:
                    count = 11;
                    break;
                default:
                    throw new NotImplementedException();
            }

            Unknown6 = new int[count];
            for (int i = 0; i < count; i++)
                Unknown6[i] = br.ReadInt32();

            Name = stream.ReadFixedSizeString(NameSize, YkdFile.NamesEncoding);
            Extension = stream.ReadFixedSizeString(ExtensionSize, YkdFile.NamesEncoding);
            Unknown17 = br.ReadInt32();
            GtexOffset = br.ReadInt32();
            Unknown18 = br.ReadInt32();
            Unknown19 = br.ReadInt32();
        }

        public void WriteToStream(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);
            bw.Write(Unknown1);
            bw.Write(DataLength);
            bw.Write(Unknown2);
            bw.Write(Unknown3);
            bw.Write(Unknown4);
            bw.Write(UnknownCounterOrType);

            for (int i = 0; i < Unknown6.Length; i++)
                bw.Write(Unknown6[i]);

            stream.WriteFixedSizeString(Name, NameSize, YkdFile.NamesEncoding);
            stream.WriteFixedSizeString(Extension, ExtensionSize, YkdFile.NamesEncoding);
            bw.Write(Unknown17);
            bw.Write(GtexOffset);
            bw.Write(Unknown18);
            bw.Write(Unknown19);
        }
    }
}