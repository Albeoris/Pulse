using System;
using System.IO;
using System.Runtime.InteropServices;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.OpenGL
{
    public static class DdsHeaderDecoder
    {
        public static DdsHeader FromFileStream(Stream input)
        {
            byte[] buff = new byte[128];
            using (SafeGCHandle handle = new SafeGCHandle(buff, GCHandleType.Pinned))
            {
                input.EnsureRead(buff, 0, buff.Length);
                return (DdsHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject() + 4, TypeCache<DdsHeader>.Type);
            }
        }

        public static DdsHeader FromGtexHeader(GtexHeader header)
        {
            DdsHeader result = DdsHeader.Create();

            result.Width = header.Width;
            result.Flags |= DdsHeaderFlags.Width;

            result.Height = header.Height;
            result.Flags |= DdsHeaderFlags.Height;

            result.Depth = header.Depth;
            result.Flags |= DdsHeaderFlags.Depth;

            result.PitchOrLinearSize = header.LinerSize;
            result.Flags |= DdsHeaderFlags.LinearSize;

            if (header.MipMapCount > 0)
            {
                result.MipMapCount = header.MipMapCount;
                result.Flags |= DdsHeaderFlags.MipMapCount;
                result.SurfaceFlags |= DdsHeaderSurfaceFlags.Mipmap | DdsHeaderSurfaceFlags.Complex;
            }

            if (header.IsCubeMap)
            {
                result.SurfaceFlags |= DdsHeaderSurfaceFlags.Complex;
                result.CubemapFlags = DdsHeaderCubemapFlags.AllCubemapFaces;
            }

            switch (header.Format)
            {
                case GtexPixelFromat.Dxt1:
                    result.PixelFormat = DdsPixelFormat.DXT1;
                    result.Flags |= DdsHeaderFlags.PixelFormat;
                    break;
                case GtexPixelFromat.Dxt3:
                    result.PixelFormat = DdsPixelFormat.DXT3;
                    result.Flags |= DdsHeaderFlags.PixelFormat;
                    break;
                case GtexPixelFromat.Dxt5:
                    result.PixelFormat = DdsPixelFormat.DXT5;
                    result.Flags |= DdsHeaderFlags.PixelFormat;
                    break;
                case GtexPixelFromat.X8R8G8B8:
                    result.PixelFormat = DdsPixelFormat.X8R8G8B8;
                    result.Flags |= DdsHeaderFlags.PixelFormat;
                    break;
                default:
                    throw new NotSupportedException();
            }

            return result;
        }
    }
}