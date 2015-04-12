using System;
using System.IO;
using System.Text;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class YkdResource : IStreamingContent
    {
        private const int NameSize = 16;

        public readonly int Size;

        public int Unknown1;
        public int Index;
        public int Dummy2;
        public int Dummy3;
        public string Name;
        public int SourceX;
        public int SourceY;
        public int SourceWidth;
        public int SourceHeight;
        public int ViewportWidth;
        public int ViewportHeight;
        public int Unknown4;
        public int Unknown5;
        public int Color1;
        public int Color2;
        public int Color3;
        public int Color4;
        public byte[] Tail;

        public YkdResource(int size)
        {
            Size = size;
        }

        public unsafe void ReadFromStream(Stream stream)
        {
            if (Size < 32)
                throw new InvalidDataException();

            BinaryReader br = new BinaryReader(stream);
            Unknown1 = br.ReadInt32();
            Index = br.ReadInt32();
            Dummy2 = br.Check(r=> r.ReadInt32(), 0);
            Dummy3 = br.Check(r=> r.ReadInt32(), 0);

            byte[] name = stream.EnsureRead(NameSize);
            fixed (byte* namePtr = &name[0])
                Name = new string((sbyte*)namePtr, 0, NameSize, YkdFile.NamesEncoding).TrimEnd('\0');

            if (Size >= 80)
            {
                SourceX = br.ReadInt32();
                SourceY = br.ReadInt32();
                SourceWidth = br.ReadInt32();
                SourceHeight = br.ReadInt32();
                ViewportWidth = br.ReadInt32();
                ViewportHeight = br.ReadInt32();
                Unknown4 = br.ReadInt32();
                Unknown5 = br.ReadInt32();
                Color1 = br.ReadInt32();
                Color2 = br.ReadInt32();
                Color3 = br.ReadInt32();
                Color4 = br.ReadInt32();
            }

            if (Size >= 96)
                Tail = stream.EnsureRead(16);
        }

        public void WriteToStream(Stream stream)
        {
            if (Size < 32)
                throw new InvalidDataException();

            BinaryWriter bw = new BinaryWriter(stream);

            bw.Write(Unknown1);
            bw.Write(Index);
            bw.Write(Dummy2);
            bw.Write(Dummy3);

            byte[] name = new byte[NameSize];
            YkdFile.NamesEncoding.GetBytes(Name, 0, Name.Length, name, 0);
            stream.Write(name, 0, name.Length);

            if (Size >= 80)
            {
                bw.Write(SourceX);
                bw.Write(SourceY);
                bw.Write(SourceWidth);
                bw.Write(SourceHeight);
                bw.Write(ViewportWidth);
                bw.Write(ViewportHeight);
                bw.Write(Unknown4);
                bw.Write(Unknown5);
                bw.Write(Color1);
                bw.Write(Color2);
                bw.Write(Color3);
                bw.Write(Color4);
            }

            if (Size >= 96)
                stream.Write(Tail, 0, Tail.Length);
        }
    }
}