using System;

namespace Pulse.Core
{
    public static class ActionExm
    {
        public static void NullSafeInvoke(this Action self)
        {
            Action h = self;
            if (h != null)
                h();
        }

        public static void NullSafeInvoke<T>(this Action<T> self, T args)
        {
            Action<T> h = self;
            if (h != null)
                h(args);
        }

        public static void NullSafeInvoke<T1, T2>(this Action<T1, T2> self, T1 arg1, T2 arg2)
        {
            Action<T1, T2> h = self;
            if (h != null)
                h(arg1, arg2);
        }
    }
}