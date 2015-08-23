using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Pulse.Core;
using Pulse.FS;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Resource = SharpDX.Direct3D11.Resource;

namespace Pulse.DirectX
{
    public static class DxTextureReader
    {
        private static readonly Dx11Device _device;
        private static readonly CreateTextureIssueWorkaround _textureCreatingWorkaround;

        private delegate void CreateTextureIssueWorkaround(DeviceContext contextRef, Resource srcTextureRef, ImageFileFormat destFormat);
        private delegate void D3DX11SaveTextureToMemory(DeviceContext contextRef, Resource srcTextureRef, ImageFileFormat destFormat, out Blob destBufOut, int flags);

        static DxTextureReader ()
        {
            Configuration.EnableObjectTracking = true;
            _device = Dx11Device.CreateDefaultAdapter();
            _textureCreatingWorkaround = InitializeWorkaround();
        }

        private static CreateTextureIssueWorkaround InitializeWorkaround()
        {
            try
            {
                D3DX11SaveTextureToMemory saveMethod = (D3DX11SaveTextureToMemory)Delegate.CreateDelegate(
                    typeof(D3DX11SaveTextureToMemory),
                    null,
                    typeof(Resource).Assembly.GetType("SharpDX.Direct3D11.D3DX11").GetMethod("SaveTextureToMemory", BindingFlags.Static | BindingFlags.NonPublic));

                return (c, t, f) =>
                {
                    Blob buff;
                    saveMethod(c, t, f, out buff, 0);
                    buff.Dispose();
                };
            }
            catch
            {
                return (c, t, f) =>
                {
                    using (MemoryStream ms = new MemoryStream(32 * 1024))
                        Resource.ToStream(c, t, f, ms);
                };
            }
        }

        public static DxTexture ReadFromWpd(WpdArchiveListing listing, WpdEntry entry)
        {
            using (Stream headers = listing.Accessor.ExtractHeaders())
            using (Stream content = listing.Accessor.ExtractContent())
            {
                headers.SetPosition(entry.Offset);

                GtexData gtex;
                SectionHeader sectionHeader = headers.ReadContent<SectionHeader>();
                switch (sectionHeader.Type)
                {
                    case SectionType.Txb:
                        gtex = ReadGtexFromTxb(headers);
                        break;
                    case SectionType.Vtex:
                        gtex = ReadGtexFromVtex(headers);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                return LoadFromStream(gtex, content);
            }
        }

        public static DxTexture LoadFromStream(GtexData gtex, Stream input)
        {
            if (gtex.Header.LayerCount == 0)
                return null;

            return gtex.Header.IsCubeMap
                ? ReadTextureCubeFromStream(gtex, input)
                : Read2DTextureFromStream(gtex, input);
        }

        private static DxTexture ReadTextureCubeFromStream(GtexData gtex, Stream input)
        {
            Texture2DDescription descriptor = GetTextureCubeDescription(gtex);

            using (SafeUnmanagedArray array = new SafeUnmanagedArray(gtex.MipMapData.Sum(d => d.Length)))
            {
                DataRectangle[] rects = new DataRectangle[gtex.MipMapData.Length];
                using (UnmanagedMemoryStream io = array.OpenStream(FileAccess.Write))
                {
                    byte[] buff = new byte[32 * 1024];
                    for (int index = 0; index < gtex.MipMapData.Length; index++)
                    {
                        GtexMipMapLocation mimMap = gtex.MipMapData[index];
                        Int32 pitch = GetPitch(descriptor, index);
                        rects[index] = CreateDataRectangle(array, io, pitch);
                        input.SetPosition(mimMap.Offset);
                        input.CopyToStream(io, mimMap.Length, buff);
                    }
                }

                Texture2D texture = new Texture2D(_device.Device, descriptor, rects);

                // Workaround
                _textureCreatingWorkaround(_device.Device.ImmediateContext, texture, ImageFileFormat.Dds);

                return new DxTexture(texture, descriptor);
            }
        }

        private static DxTexture Read2DTextureFromStream(GtexData gtex, Stream input)
        {
            Texture2DDescription descriptor = Get2DTextureDescription(gtex);

            using (SafeUnmanagedArray array = new SafeUnmanagedArray(gtex.MipMapData.Sum(d => d.Length)))
            {
                DataRectangle[] rects = new DataRectangle[gtex.MipMapData.Length];
                using (UnmanagedMemoryStream io = array.OpenStream(FileAccess.Write))
                {
                    byte[] buff = new byte[32 * 1024];
                    for (int index = 0; index < gtex.MipMapData.Length; index++)
                    {
                        GtexMipMapLocation mimMap = gtex.MipMapData[index];
                        Int32 pitch = GetPitch(descriptor, index);
                        rects[index] = CreateDataRectangle(array, io, pitch);
                        input.SetPosition(mimMap.Offset);
                        input.CopyToStream(io, mimMap.Length, buff);
                    }
                }
                
                Texture2D texture = new Texture2D(_device.Device, descriptor, rects);

                // Workaround
                _textureCreatingWorkaround(_device.Device.ImmediateContext, texture, ImageFileFormat.Dds);

                return new DxTexture(texture, descriptor);
            }
        }

        private static Int32 GetPitch(Texture2DDescription descriptor, Int32 mipLevel)
        {
            return GetRowPitch(descriptor.Format, descriptor.Width, mipLevel);
        }

        private static void GetPitch(Texture3DDescription descriptor, int mipLevel, out int rowPitch, out int depthPitch)
        {
            rowPitch = GetRowPitch(descriptor.Format, descriptor.Width, mipLevel);
            depthPitch = rowPitch * 6;
        }

        private static Int32 GetRowPitch(Format format, Int32 width, Int32 mipLevel)
        {
            for (int i = 0; i < mipLevel; i++)
                width >>= 1;

            if (width == 0)
                width = 1;

            switch (format)
            {
                case Format.BC1_UNorm:
                    return Math.Max(1, ((width + 3) / 4)) * 8;
                case Format.BC2_UNorm_SRgb:
                case Format.BC3_UNorm_SRgb:
                    return Math.Max(1, ((width + 3) / 4)) * 16;
            }

            return width * FormatHelper.SizeOfInBytes(format);
        }

        private static DataRectangle CreateDataRectangle(SafeUnmanagedArray array, UnmanagedMemoryStream input, Int32 pitch)
        {
            return new DataRectangle(new IntPtr(array.DangerousGetHandle().ToInt64() + input.Position), pitch);
        }

        private static DataBox CreateDataBox(SafeUnmanagedArray array, UnmanagedMemoryStream input, Int32 rowPitch, Int32 depthPitch)
        {
            return new DataBox(new IntPtr(array.DangerousGetHandle().ToInt64() + input.Position), rowPitch, depthPitch);
        }

        private static Texture2DDescription GetTextureCubeDescription(GtexData gtex)
        {
            if (!gtex.Header.IsCubeMap)
                throw new Exception("IsCubeMap: false");

            return new Texture2DDescription
            {
                ArraySize = 6,
                Width = gtex.Header.Width,
                Height = gtex.Header.Height,
                Format = GetDxFormat(gtex.Header.Format),
                
                MipLevels = gtex.Header.MipMapCount,
                
                BindFlags = BindFlags.ShaderResource,
                OptionFlags = ResourceOptionFlags.Shared | ResourceOptionFlags.TextureCube,
                
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                SampleDescription = new SampleDescription(1, 0)
            };
        }

        private static Texture2DDescription Get2DTextureDescription(GtexData gtex)
        {
            if (gtex.Header.IsCubeMap)
                throw new Exception("IsCubeMap: true");

            return new Texture2DDescription
            {
                ArraySize = 1,
                Width = gtex.Header.Width,
                Height = gtex.Header.Height,
                Format = GetDxFormat(gtex.Header.Format),
                
                MipLevels = gtex.Header.MipMapCount,
                
                BindFlags = BindFlags.ShaderResource,
                OptionFlags = ResourceOptionFlags.Shared,
                
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                SampleDescription = new SampleDescription(1, 0)
            };
        }

        private static Format GetDxFormat(GtexPixelFromat format)
        {
            switch (format)
            {
                case GtexPixelFromat.Dxt1:
                    return Format.BC1_UNorm;
                case GtexPixelFromat.Dxt3:
                    return Format.BC2_UNorm_SRgb;
                case GtexPixelFromat.Dxt5:
                    return Format.BC3_UNorm_SRgb;
                case GtexPixelFromat.A8R8G8B8:
                    return Format.B8G8R8A8_UNorm;
                case GtexPixelFromat.X8R8G8B8:
                    return Format.B8G8R8X8_UNorm;
            }

            throw new NotImplementedException(format.ToString());
        }

        private static GtexData ReadGtexFromTxb(Stream headers)
        {
            TextureHeader textureHeader = headers.ReadContent<TextureHeader>();
            return headers.ReadContent<GtexData>();
        }

        private static GtexData ReadGtexFromVtex(Stream headers)
        {
            VtexHeader textureHeader = headers.ReadContent<VtexHeader>();
            headers.Seek(textureHeader.GtexOffset - VtexHeader.Size, SeekOrigin.Current);
            return headers.ReadContent<GtexData>();
        }
    }
}