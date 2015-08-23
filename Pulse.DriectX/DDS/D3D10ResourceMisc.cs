using System;

namespace Pulse.DirectX
{
    /// <summary>
    /// Identifies other, less common options for resources.
    /// http://msdn.microsoft.com/en-us/library/windows/desktop/bb172412%28v=vs.85%29.aspx
    /// </summary>
    [Flags]
    public enum D3D10ResourceMisc
    {
        GenerateMips = 0x1,
        Shared = 0x2,
        TextureCube = 0x4,
        SharedKeyedMutex = 0x10,
        GdiCompatible = 0x20
    }
}