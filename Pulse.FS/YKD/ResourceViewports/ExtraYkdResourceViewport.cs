using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class ExtraYkdResourceViewport : YkdResourceViewport
    {
        public int SourceX;
        public int SourceY;
        public int SourceWidth;
        public int SourceHeight;
        public int ViewportWidth;
        public int ViewportHeight;
        public YkdResourceFlags Flags;
        public int Unknown5;
        public int UpperLeftColor;
        public int BottomLeftColor;
        public int UpperRightColor;
        public int BottomRightColor;
        public byte[] Tail;

        public override YkdResourceViewportType Type
        {
            get { return YkdResourceViewportType.Extra; }
        }

        public override int Size
        {
            get { return 64; }
        }

        public override void ReadFromStream(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);

            SourceX = br.ReadInt32();
            SourceY = br.ReadInt32();
            SourceWidth = br.ReadInt32();
            SourceHeight = br.ReadInt32();
            ViewportWidth = br.ReadInt32();
            ViewportHeight = br.ReadInt32();
            Flags = (YkdResourceFlags)br.ReadInt32();
            Unknown5 = br.ReadInt32();
            UpperLeftColor = br.ReadInt32();
            BottomLeftColor = br.ReadInt32();
            UpperRightColor = br.ReadInt32();
            BottomRightColor = br.ReadInt32();
            Tail = stream.EnsureRead(16);
        }

        public override void WriteToStream(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);

            bw.Write(SourceX);
            bw.Write(SourceY);
            bw.Write(SourceWidth);
            bw.Write(SourceHeight);
            bw.Write(ViewportWidth);
            bw.Write(ViewportHeight);
            bw.Write((int)Flags);
            bw.Write(Unknown5);
            bw.Write(UpperLeftColor);
            bw.Write(BottomLeftColor);
            bw.Write(UpperRightColor);
            bw.Write(BottomRightColor);
            bw.Write(Tail);
        }

        public override YkdResourceViewport Clone()
        {
            return new ExtraYkdResourceViewport
            {
                SourceX = SourceX,
                SourceY = SourceY,
                SourceWidth = SourceWidth,
                SourceHeight = SourceHeight,
                ViewportWidth = ViewportWidth,
                ViewportHeight = ViewportHeight,
                Flags = Flags,
                Unknown5 = Unknown5,
                UpperLeftColor = UpperLeftColor,
                BottomLeftColor = BottomLeftColor,
                UpperRightColor = UpperRightColor,
                BottomRightColor = BottomRightColor,
                Tail = (byte[])Tail.Clone()
            };
        }
    }
}