using System;
using System.IO;
using System.Runtime.InteropServices;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.OpenGL
{
    public static class DdsHeaderEncoder
    {
        public static void ToFileStream(DdsHeader header, Stream output)
        {
            byte[] buff = new byte[128];
            using (SafeGCHandle handle = new SafeGCHandle(buff, GCHandleType.Pinned))
            {
                IntPtr ptr = handle.AddrOfPinnedObject();

                Marshal.WriteInt32(ptr, DdsHeader.MagicHeader);
                Marshal.StructureToPtr(header, ptr + 4, false);

                output.Write(buff, 0, buff.Length);
            }
        }

        public static void ToGtexHeader(DdsHeader header, GtexHeader output)
        {
            output.Width = (short)header.Width;
            output.Height = (short)header.Height;
            //output.Depth = (short)header.Depth;
            if (header.MipMapCount > 1)
                throw new NotImplementedException();
            //output.LinerSize = header.PitchOrLinearSize;

            //if (header.MipMapCount > 0)
            //    output.MipMapCount = (byte)header.MipMapCount;

            if ((header.CubemapFlags & DdsHeaderCubemapFlags.Cubemap) == DdsHeaderCubemapFlags.Cubemap)
                output.IsCubeMap = true;

            if (header.PixelFormat.Equals(DdsPixelFormat.DXT1))
                output.Format = GtexPixelFromat.Dxt1;
            else if (header.PixelFormat.Equals(DdsPixelFormat.DXT3))
                output.Format = GtexPixelFromat.Dxt3;
            else if (header.PixelFormat.Equals(DdsPixelFormat.DXT5))
                output.Format = GtexPixelFromat.Dxt5;
            else if (header.PixelFormat.Equals(DdsPixelFormat.X8R8G8B8))
                output.Format = GtexPixelFromat.X8R8G8B8;
            else
                throw new NotImplementedException();
        }
    }
}