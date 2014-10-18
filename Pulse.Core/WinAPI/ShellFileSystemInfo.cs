using System;
using System.Runtime.InteropServices;

namespace Pulse.Core
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ShellFileSystemInfo
    {
        public IntPtr IconHandle;
        public IntPtr IcondIndex;
        public uint Attributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string DisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)] public string TypeName;
    }
}