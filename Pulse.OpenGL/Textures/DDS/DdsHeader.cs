using System.Runtime.InteropServices;

namespace Pulse.OpenGL
{
    /// <summary>
    /// Describes a DDS file header.
    /// http://msdn.microsoft.com/en-us/library/windows/desktop/bb943982%28v=vs.85%29.aspx
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DdsHeader
    {
        /// <summary>
        /// Magic code to identify DDS header
        /// </summary>
        public const int MagicHeader = 0x20534444; // "DDS "

        public static DdsHeader Create()
        {
            return new DdsHeader
            {
                Size = 124,
                Flags = DdsHeaderFlags.Caps,
                SurfaceFlags = DdsHeaderSurfaceFlags.Texture
            };
        }

        /// <summary>
        /// Size of structure. This member must be set to 124.
        /// </summary>
        private int Size;

        /// <summary>
        /// Flags to indicate which members contain valid data. 
        /// </summary>
        public DdsHeaderFlags Flags;

        /// <summary>
        /// Surface height (in pixels).
        /// </summary>
        public int Height;

        /// <summary>
        /// Surface width (in pixels).
        /// </summary>
        public int Width;

        /// <summary>
        /// The pitch or number of bytes per scan line in an uncompressed texture; the total number of bytes in the top level texture for a compressed texture.
        /// For information about how to compute the pitch, see the DDS File Layout section of the Programming Guide for DDS.
        /// </summary>
        public int PitchOrLinearSize;

        /// <summary>
        /// Depth of a volume texture (in pixels), otherwise unused. 
        /// </summary>
        public int Depth;

        /// <summary>
        /// Number of mipmap levels, otherwise unused.
        /// </summary>
        public int MipMapCount;

        /// <summary>
        /// Unused.
        /// </summary>
        private unsafe fixed int Reserved1 [11];

        /// <summary>
        /// The pixel format
        /// </summary>
        public DdsPixelFormat PixelFormat;

        /// <summary>
        /// Specifies the complexity of the surfaces stored.
        /// </summary>
        public DdsHeaderSurfaceFlags SurfaceFlags;

        /// <summary>
        /// Additional detail about the surfaces stored.
        /// </summary>
        public DdsHeaderCubemapFlags CubemapFlags;

        /// <summary>
        /// Unused
        /// </summary>
        public int Caps3;

        /// <summary>
        /// Unused
        /// </summary>
        public int Caps4;

        /// <summary>
        /// Unused
        /// </summary>
        public int Reserved2;
    }
}