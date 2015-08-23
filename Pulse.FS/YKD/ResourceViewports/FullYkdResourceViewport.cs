using System.IO;
using Pulse.Core;

namespace Pulse.FS
{
    public sealed class FullYkdResourceViewport : YkdResourceViewport
    {
        public byte[] UnknownData;
        public int ViewportWidth;
        public int ViewportHeight;
        public int UpperLeftColor;
        public int BottomLeftColor;
        public int UpperRightColor;
        public int BottomRightColor;
        public int Unknown1;
        public int Unknown2;

        public override YkdResourceViewportType Type
        {
            get { return YkdResourceViewportType.Full; }
        }

        public override int Size
        {
            get { return 48; }
        }

        public override void ReadFromStream(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);

            UnknownData = stream.EnsureRead(16);
            ViewportWidth = br.ReadInt32();
            ViewportHeight = br.ReadInt32();
            UpperLeftColor = br.ReadInt32();
            BottomLeftColor = br.ReadInt32();
            UpperRightColor = br.ReadInt32();
            BottomRightColor = br.ReadInt32();
            Unknown1 = br.ReadInt32();
            Unknown2 = br.ReadInt32();
        }

        public override void WriteToStream(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);

            bw.Write(UnknownData);
            bw.Write(ViewportWidth);
            bw.Write(ViewportHeight);
            bw.Write(UpperLeftColor);
            bw.Write(BottomLeftColor);
            bw.Write(UpperRightColor);
            bw.Write(BottomRightColor);
            bw.Write(Unknown1);
            bw.Write(Unknown2);
        }

        public override YkdResourceViewport Clone()
        {
            return new FullYkdResourceViewport
            {
                UnknownData = (byte[])UnknownData.Clone(),
                ViewportWidth = ViewportWidth,
                ViewportHeight = ViewportHeight,
                UpperLeftColor = UpperLeftColor,
                BottomLeftColor = BottomLeftColor,
                UpperRightColor = UpperRightColor,
                BottomRightColor = BottomRightColor,
                Unknown1 = Unknown1,
                Unknown2 = Unknown2
            };
        }
    }
}