using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Pulse.Core;

namespace Pulse.FS
{
    public enum GtexPixelFromat : byte
    {
        X8R8G8B8 = 0x03,
        A8R8G8B8 = 0x04,
        Dxt1 = 0x18,
        Dxt3 = 0x19,
        Dxt5 = 0x1A
    }

    public sealed class GtexHeader : IStreamingContent
    {
        private const uint MagicValue = 0x47544558;

        public uint Magic;
        public byte Unknown1;
        public byte Unknown2;
        public GtexPixelFromat Format;
        public byte MipMapCount;
        public byte Unknown3;
        public bool IsCubeMap;
        public short Width;
        public short Height;
        public short Depth;
        public int LinerSize;
        public int DataOffset;

        public int LayerCount
        {
          get { return IsCubeMap ? 6 * MipMapCount : MipMapCount; }
        }

        public void ReadFromStream(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            Magic = br.ReadBigUInt32();
            Unknown1 = br.ReadByte();
            Unknown2 = br.ReadByte();
            Format = (GtexPixelFromat)br.ReadByte();
            MipMapCount = br.ReadByte();
            Unknown3 = br.ReadByte();
            IsCubeMap = br.ReadByte() == 1;
            Width = br.ReadBigInt16();
            Height = br.ReadBigInt16();
            Depth = br.ReadBigInt16();
            LinerSize = br.ReadBigInt32();
            DataOffset = br.ReadBigInt32();
        }

        public void WriteToStream(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);
            bw.WriteBig(Magic);
            bw.Write(Unknown1);
            bw.Write(Unknown2);
            bw.Write((byte)Format);
            bw.Write(MipMapCount);
            bw.Write(Unknown3);
            bw.Write((byte)(IsCubeMap ? 1 : 0));
            bw.WriteBig(Width);
            bw.WriteBig(Height);
            bw.WriteBig(Depth);
            bw.WriteBig(LinerSize);
            bw.WriteBig(DataOffset);
        }
    }
}
