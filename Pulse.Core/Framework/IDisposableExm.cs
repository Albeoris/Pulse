using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace Pulse.Core
{
    public static class IDisposableExm
    {
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NullSafeDispose(this IDisposable self)
        {
            if (!ReferenceEquals(self, null))
                self.Dispose();
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NullSafeDispose<T>(this Lazy<T> self) where T : IDisposable
        {
            if (!ReferenceEquals(self, null) && self.IsValueCreated)
                self.Value.NullSafeDispose();
        }

        public static void SafeDispose(this IDisposable self)
        {
            try
            {
                if (!ReferenceEquals(self, null))
                    self.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                Debug.Fail(ex.Message, ex.ToString());
            }
        }

        public static void NullSafeDispose(this IEnumerable<IDisposable> self)
        {
            if (ReferenceEquals(self, null))
                return;

            foreach (IDisposable item in self)
                NullSafeDispose(item);
        }

        public static void SafeDispose(this IEnumerable<IDisposable> self)
        {
            if (ReferenceEquals(self, null))
                return;

            foreach (IDisposable item in self)
                SafeDispose(item);
        }
    }
}