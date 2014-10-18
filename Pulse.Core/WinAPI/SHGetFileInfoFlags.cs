using System;

namespace Pulse.Core
{
    [Flags]
    public enum SHGetFileInfoFlags : uint
    {
        Icon = 0x100,
        LargeIcon = 0x0,
        SmallIcon = 0x1
    }
}