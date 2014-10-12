using System;
using System.Runtime.InteropServices;

namespace Pulse.Core
{
    public sealed class SafeGCHandle : SafeHandle
    {
        public SafeGCHandle(object target, GCHandleType type)
            : base(IntPtr.Zero, true)
        {
            SetHandle(GCHandle.ToIntPtr(GCHandle.Alloc(target, type)));
        }

        public GCHandle Handle
        {
            get { return GCHandle.FromIntPtr(handle); }
        }

        public override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        protected override bool ReleaseHandle()
        {
            try
            {
                GCHandle.FromIntPtr(handle).Free();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public IntPtr AddrOfPinnedObject()
        {
            return Handle.AddrOfPinnedObject();
        }
    }
}