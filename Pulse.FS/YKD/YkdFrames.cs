using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class YkdFrames : IStreamingContent
    {
        public int Unknown1;
        public int Unknown2;
        public int Unknown3;

        public YkdFrame[] Frames;

        public int Count
        {
            get { return Frames.Length; }
        }

        public YkdFrame this[int index]
        {
            get { return Frames[index]; }
            set { Frames[index] = value; }
        }

        public int CalcSize()
        {
            int count = Count;
            int alignment = ((4 - (count % 4)) % 4);

            return ((4 + count + alignment) * 4) + count * YkdFrame.Size;
        }

        public void ReadFromStream(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);

            Unknown1 = br.ReadInt32();
            Unknown2 = br.ReadInt32();
            int count = br.ReadInt32();
            Unknown3 = br.ReadInt32();

            int[] offsets = new int[count];
            for (int i = 0; i < count; i++)
                offsets[i] = br.ReadInt32();

            int alignment = ((4 - (count % 4)) % 4);
            for (int i = 0; i < alignment; i++)
                br.Check(r => r.ReadInt32(), 0);

            Frames = new YkdFrame[count];
            for (int i = 0; i < count; i++)
            {
                stream.SetPosition(offsets[i]);
                Frames[i] = stream.ReadContent<YkdFrame>();
            }
        }

        public void WriteToStream(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);

            int count = Count;
            int alignment = ((4 - (count % 4)) % 4);

            bw.Write(Unknown1);
            bw.Write(Unknown2);
            bw.Write(count);
            bw.Write(Unknown3);

            int offset = (int)stream.Position + (count + alignment) * 4;

            for (int i = 0; i < count; i++)
                bw.Write(offset + i * YkdFrame.Size);
            for (int i = 0; i < alignment; i++)
                bw.Write(0);

            stream.WriteContent(Frames);
        }
    }
}