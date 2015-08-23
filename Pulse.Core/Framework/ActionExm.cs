using System;

namespace Pulse.Core
{
    public static class ActionExm
    {
        public static void NullSafeInvoke(this Action self)
        {
            self?.Invoke();
        }

        public static void NullSafeInvoke<T>(this Action<T> self, T args)
        {
            self?.Invoke(args);
        }

        public static void NullSafeInvoke<T1, T2>(this Action<T1, T2> self, T1 arg1, T2 arg2)
        {
            self?.Invoke(arg1, arg2);
        }
    }
}