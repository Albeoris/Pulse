using System;
using System.IO;
using System.Linq;
using System.Text;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class YkdBlockEntry : IStreamingContent
    {
        private const int NameSize = 16;

        public string Name;
        public YkdOffsets Offsets;
        public YkdFrames[] Frames;

        public int CalcSize()
        {
            YkdFrames[] frames = Frames ?? new YkdFrames[0];
            YkdOffsets offsets = new YkdOffsets {Offsets = new int[frames.Length]};

            return NameSize + offsets.CalcSize() + frames.Sum(t => t.CalcSize());
        }

        public unsafe void ReadFromStream(Stream stream)
        {
            byte[] name = stream.EnsureRead(NameSize);
            fixed (byte* namePtr = &name[0])
                Name = new string((sbyte*)namePtr, 0, NameSize, YkdFile.NamesEncoding).TrimEnd('\0');

            Offsets = stream.ReadContent<YkdOffsets>();
            Frames = new YkdFrames[Offsets.Count];
            for (int i = 0; i < Frames.Length; i++)
                Frames[i] = stream.ReadContent<YkdFrames>();
        }

        public void WriteToStream(Stream stream)
        {
            byte[] name = new byte[NameSize];
            YkdFile.NamesEncoding.GetBytes(Name, 0, Name.Length, name, 0);
            stream.Write(name, 0, name.Length);

            YkdOffsets.WriteToStream(stream, ref Offsets, ref Frames, b => b.CalcSize());
            stream.WriteContent(Frames);
        }
    }
}