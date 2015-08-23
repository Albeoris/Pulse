using System;
using System.Runtime.InteropServices;

namespace Pulse.OpenGL
{
    /// <summary>
    /// Surface pixel format.
    /// http://msdn.microsoft.com/en-us/library/windows/desktop/bb943984%28v=vs.85%29.aspx
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DdsPixelFormat
    {
        public DdsPixelFormat(DdsPixelFormatFlags flags, int fourCc, int rgbBitCount, uint redBitMask, uint greenBitMask, uint blueBitMask, uint alphaBitMask)
        {
            Size = 32;
            Flags = flags;
            FourCC = fourCc;
            RGBBitCount = rgbBitCount;
            RedBitMask = redBitMask;
            GreenBitMask = greenBitMask;
            BlueBitMask = blueBitMask;
            AlphaBitMask = alphaBitMask;
        }

        /// <summary>
        /// Structure size; set to 32 (bytes).
        /// </summary>
        public int Size;

        /// <summary>
        /// Values which indicate what type of data is in the surface. 
        /// </summary>
        public DdsPixelFormatFlags Flags;

        /// <summary>
        /// Four-character codes for specifying compressed or custom formats. Possible values include: DXT1, DXT2, DXT3, DXT4, or DXT5.
        /// A FourCC of DX10 indicates the prescense of the DDS_HEADER_DXT10 extended header, and the dxgiFormat member of that structure indicates the true format.
        /// When using a four-character code, dwFlags must include DDPF_FOURCC.
        /// </summary>
        public int FourCC;

        /// <summary>
        /// Number of bits in an RGB (possibly including alpha) format. Valid when dwFlags includes DDPF_RGB, DDPF_LUMINANCE, or DDPF_YUV.
        /// </summary>
        public int RGBBitCount;

        /// <summary>
        /// Red (or lumiannce or Y) mask for reading color data. For instance, given the A8R8G8B8 format, the red mask would be 0x00ff0000.
        /// </summary>
        public uint RedBitMask;

        /// <summary>
        /// Green (or U) mask for reading color data. For instance, given the A8R8G8B8 format, the green mask would be 0x0000ff00.
        /// </summary>
        public uint GreenBitMask;

        /// <summary>
        /// Blue (or V) mask for reading color data. For instance, given the A8R8G8B8 format, the blue mask would be 0x000000ff.
        /// </summary>
        public uint BlueBitMask;

        /// <summary>
        /// Alpha mask for reading alpha data. dwFlags must include DDPF_ALPHAPIXELS or DDPF_ALPHA. For instance, given the A8R8G8B8 format, the alpha mask would be 0xff000000.
        /// </summary>
        public uint AlphaBitMask;

        public static readonly DdsPixelFormat DXT1 = new DdsPixelFormat(DdsPixelFormatFlags.FourCC, new DdsPixelFormatFourDescriptor('D', 'X', 'T', '1'), 0, 0, 0, 0, 0);
        public static readonly DdsPixelFormat DXT2 = new DdsPixelFormat(DdsPixelFormatFlags.FourCC, new DdsPixelFormatFourDescriptor('D', 'X', 'T', '2'), 0, 0, 0, 0, 0);
        public static readonly DdsPixelFormat DXT3 = new DdsPixelFormat(DdsPixelFormatFlags.FourCC, new DdsPixelFormatFourDescriptor('D', 'X', 'T', '3'), 0, 0, 0, 0, 0);
        public static readonly DdsPixelFormat DXT4 = new DdsPixelFormat(DdsPixelFormatFlags.FourCC, new DdsPixelFormatFourDescriptor('D', 'X', 'T', '4'), 0, 0, 0, 0, 0);
        public static readonly DdsPixelFormat DXT5 = new DdsPixelFormat(DdsPixelFormatFlags.FourCC, new DdsPixelFormatFourDescriptor('D', 'X', 'T', '5'), 0, 0, 0, 0, 0);
        public static readonly DdsPixelFormat BC4_UNorm = new DdsPixelFormat(DdsPixelFormatFlags.FourCC, new DdsPixelFormatFourDescriptor('B', 'C', '4', 'U'), 0, 0, 0, 0, 0);
        public static readonly DdsPixelFormat BC4_SNorm = new DdsPixelFormat(DdsPixelFormatFlags.FourCC, new DdsPixelFormatFourDescriptor('B', 'C', '4', 'S'), 0, 0, 0, 0, 0);
        public static readonly DdsPixelFormat BC5_UNorm = new DdsPixelFormat(DdsPixelFormatFlags.FourCC, new DdsPixelFormatFourDescriptor('B', 'C', '5', 'U'), 0, 0, 0, 0, 0);
        public static readonly DdsPixelFormat BC5_SNorm = new DdsPixelFormat(DdsPixelFormatFlags.FourCC, new DdsPixelFormatFourDescriptor('B', 'C', '5', 'S'), 0, 0, 0, 0, 0);
        public static readonly DdsPixelFormat R8G8_B8G8 = new DdsPixelFormat(DdsPixelFormatFlags.FourCC, new DdsPixelFormatFourDescriptor('R', 'G', 'B', 'G'), 0, 0, 0, 0, 0);
        public static readonly DdsPixelFormat G8R8_G8B8 = new DdsPixelFormat(DdsPixelFormatFlags.FourCC, new DdsPixelFormatFourDescriptor('G', 'R', 'G', 'B'), 0, 0, 0, 0, 0);
        public static readonly DdsPixelFormat A8R8G8B8 = new DdsPixelFormat(DdsPixelFormatFlags.RGB | DdsPixelFormatFlags.AlphaPixels, 0, 32, 0x00ff0000, 0x0000ff00, 0x000000ff, 0xff000000);
        public static readonly DdsPixelFormat X8R8G8B8 = new DdsPixelFormat(DdsPixelFormatFlags.RGB, 0, 32, 0x00ff0000, 0x0000ff00, 0x000000ff, 0x00000000);
        public static readonly DdsPixelFormat A8B8G8R8 = new DdsPixelFormat(DdsPixelFormatFlags.RGB | DdsPixelFormatFlags.AlphaPixels, 0, 32, 0x000000ff, 0x0000ff00, 0x00ff0000, 0xff000000);
        public static readonly DdsPixelFormat X8B8G8R8 = new DdsPixelFormat(DdsPixelFormatFlags.RGB, 0, 32, 0x000000ff, 0x0000ff00, 0x00ff0000, 0x00000000);
        public static readonly DdsPixelFormat G16R16 = new DdsPixelFormat(DdsPixelFormatFlags.RGB, 0, 32, 0x0000ffff, 0xffff0000, 0x00000000, 0x00000000);
        public static readonly DdsPixelFormat R5G6B5 = new DdsPixelFormat(DdsPixelFormatFlags.RGB, 0, 16, 0x0000f800, 0x000007e0, 0x0000001f, 0x00000000);
        public static readonly DdsPixelFormat A1R5G5B5 = new DdsPixelFormat(DdsPixelFormatFlags.RGB | DdsPixelFormatFlags.AlphaPixels, 0, 16, 0x00007c00, 0x000003e0, 0x0000001f, 0x00008000);
        public static readonly DdsPixelFormat A4R4G4B4 = new DdsPixelFormat(DdsPixelFormatFlags.RGB | DdsPixelFormatFlags.AlphaPixels, 0, 16, 0x00000f00, 0x000000f0, 0x0000000f, 0x0000f000);
        public static readonly DdsPixelFormat R8G8B8 = new DdsPixelFormat(DdsPixelFormatFlags.RGB, 0, 24, 0x00ff0000, 0x0000ff00, 0x000000ff, 0x00000000);
        public static readonly DdsPixelFormat L8 = new DdsPixelFormat(DdsPixelFormatFlags.Luminance, 0, 8, 0xff, 0x00, 0x00, 0x00);
        public static readonly DdsPixelFormat L16 = new DdsPixelFormat(DdsPixelFormatFlags.Luminance, 0, 16, 0xffff, 0x0000, 0x0000, 0x0000);
        public static readonly DdsPixelFormat A8L8 = new DdsPixelFormat(DdsPixelFormatFlags.Luminance | DdsPixelFormatFlags.AlphaPixels, 0, 16, 0x00ff, 0x0000, 0x0000, 0xff00);
        public static readonly DdsPixelFormat A8 = new DdsPixelFormat(DdsPixelFormatFlags.Alpha, 0, 8, 0x00, 0x00, 0x00, 0xff);
        public static readonly DdsPixelFormat DX10 = new DdsPixelFormat(DdsPixelFormatFlags.FourCC, new DdsPixelFormatFourDescriptor('D', 'X', '1', '0'), 0, 0, 0, 0, 0);
    }
}