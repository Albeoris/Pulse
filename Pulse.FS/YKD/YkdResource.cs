using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class YkdResource : IStreamingContent
    {
        private const int NameSize = 16;

        public YkdResourceViewportType Type;
        public int Index;
        public int Dummy2;
        public int Dummy3;
        public string Name;
        public YkdResourceViewport Viewport;

        public int CalcSize()
        {
            return 32 + Viewport.Size;
        }

        public void ReadFromStream(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            Type = (YkdResourceViewportType)br.ReadInt32();
            Index = br.ReadInt32();
            Dummy2 = br.Check(r => r.ReadInt32(), 0);
            Dummy3 = br.Check(r => r.ReadInt32(), 0);
            Name = stream.ReadFixedSizeString(NameSize, YkdFile.NamesEncoding);
            Viewport = YkdResourceViewport.ReadFromStream(Type, stream);
        }

        public void WriteToStream(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);

            bw.Write((int)Type);
            bw.Write(Index);
            bw.Write(Dummy2);
            bw.Write(Dummy3);

            byte[] name = new byte[NameSize];
            YkdFile.NamesEncoding.GetBytes(Name, 0, Name.Length, name, 0);
            stream.Write(name, 0, name.Length);

            Viewport.WriteToStream(stream);
        }

        public YkdResource Clone()
        {
            return new YkdResource
            {
                Type = Type,
                Index = Index,
                Dummy2 = Dummy2,
                Dummy3 = Dummy3,
                Name = Name,
                Viewport = Viewport.Clone()
            };
        }
    }
}