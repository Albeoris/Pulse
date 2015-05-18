using System;
using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class YkdOffsets : IStreamingContent
    {
        public int Unknown1; // gr103.ykd Resources
        public int Unknown2;
        public int Unknown3; // Animation

        public int[] Offsets;

        public int Count
        {
            get { return Offsets.Length; }
        }

        public int this[int index]
        {
            get { return Offsets[index]; }
            set { Offsets[index] = value; }
        }

        public int CalcSize()
        {
            return (4 + Count + (4 - (Count % 4)) % 4) * 4;
        }

        public void ReadFromStream(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);

            int count = br.ReadInt32();
            Unknown1 = br.ReadInt32();
            Unknown2 = br.ReadInt32();
            Unknown3 = br.ReadInt32();

            Offsets = new int[count];
            for (int i = 0; i < count; i++)
            {
                int offset = br.ReadInt32();
                Offsets[i] = offset;
                if (offset == 0 || offset >= stream.Length)
                    throw new InvalidDataException();
            }

            int alignment = ((4 - (count % 4)) % 4);
            for (int i = 0; i < alignment; i++)
                br.Check(r => r.ReadInt32(), 0);
        }

        public void WriteToStream(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);

            bw.Write(Count);
            bw.Write(Unknown1);
            bw.Write(Unknown2);
            bw.Write(Unknown3);

            for (int i = 0; i < Count; i++)
                bw.Write(Offsets[i]);

            int alignment = ((4 - (Count % 4)) % 4);
            for (int i = 0; i < alignment; i++)
                bw.Write(0);
        }

        public static void WriteToStream<T>(Stream stream, ref YkdOffsets self, ref T[] values, Func<T, int> valueSizeCalculator)
        {
            if (values == null) values = new T[0];
            if (self == null) self = new YkdOffsets();

            self.Offsets = new int[values.Length];
            int offset = (int)stream.Position + self.CalcSize();
            for (int i = 0; i < self.Count; i++)
            {
                self[i] = offset;
                offset += valueSizeCalculator(values[i]);
            }

            stream.WriteContent(self);
        }

        public void Insert(int index, int size)
        {
            int[] offsets = new int[Offsets.Length + 1];
            Array.Copy(Offsets, offsets, index);
            for (int i = index + 1; i < offsets.Length; i++)
                offsets[i] = Offsets[i - 1] + size;
            Offsets = offsets;
        }

        public void Remove(int index)
        {
            int[] offsets = new int[Offsets.Length - 1];
            int size = Offsets[index];
            Array.Copy(Offsets, offsets, index);
            for (int i = index; i < offsets.Length - 1; i++)
                offsets[i] = Offsets[i + 1] - size;
            Offsets = offsets;
        }
    }
}