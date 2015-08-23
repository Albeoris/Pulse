using System;

namespace Pulse.OpenGL
{
    /// <summary>
    /// Identifies the type of resource being used.
    /// http://msdn.microsoft.com/en-us/library/windows/desktop/bb172411%28v=vs.85%29.aspx
    /// </summary>
    [Flags]
    public enum D3D10ResourceDimension
    {
        /// <summary>
        /// Resource is of unknown type.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Resource is a buffer.
        /// </summary>
        Buffer = 1,

        /// <summary>
        /// Resource is a 1D texture.
        /// </summary>
        Texture1D = 2,

        /// <summary>
        /// Resource is a 2D texture.
        /// </summary>
        Texture2D = 3,

        /// <summary>
        /// Resource is a 3D texture.
        /// </summary>
        Texture3D = 4
    }
}