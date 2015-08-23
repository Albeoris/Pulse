using System;

namespace Pulse.OpenGL
{
    /// <summary>
    /// Specifies the complexity of the surfaces stored.
    /// http://msdn.microsoft.com/en-us/library/windows/desktop/bb943982%28v=vs.85%29.aspx
    /// </summary>
    [Flags]
    public enum DdsHeaderSurfaceFlags
    {
        /// <summary>
        /// Optional; must be used on any file that contains more than one surface (a mipmap, a cubic environment map, or mipmapped volume texture).
        /// </summary>
        Complex = 0x8,

        /// <summary>
        /// Optional; should be used for a mipmap.
        /// </summary>
        Mipmap = 0x400000,

        /// <summary>
        /// Required
        /// </summary>
        Texture = 0x1000
    }
}