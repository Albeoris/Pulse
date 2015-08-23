using System;

namespace Pulse.OpenGL
{
    /// <summary>
    /// Additional detail about the surfaces stored.
    /// http://msdn.microsoft.com/en-us/library/windows/desktop/bb943982%28v=vs.85%29.aspx
    /// </summary>
    [Flags]
    public enum DdsHeaderCubemapFlags
    {
        /// <summary>
        /// Required for a volume texture.
        /// </summary>
        Volume = 0x200000,

        /// <summary>
        /// Required for a cube map
        /// </summary>
        Cubemap = 0x200,

        /// <summary>
        /// Required when these surfaces are stored in a cube map.
        /// </summary>
        PositiveX = 0x400,

        /// <summary>
        /// Required when these surfaces are stored in a cube map.
        /// </summary>
        NegativeX = 0x800,

        /// <summary>
        /// Required when these surfaces are stored in a cube map.
        /// </summary>
        PositiveY = 0x1000,

        /// <summary>
        /// Required when these surfaces are stored in a cube map.
        /// </summary>
        NegativeY = 0x2000,

        /// <summary>
        /// Required when these surfaces are stored in a cube map.
        /// </summary>
        PositiveZ = 0x4000,

        /// <summary>
        /// Required when these surfaces are stored in a cube map.
        /// </summary>
        NegativeZ = 0x8000,

        AllCubemapFaces = PositiveX | NegativeX | PositiveY | NegativeY | PositiveZ | NegativeZ
    }
}