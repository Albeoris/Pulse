using System;

namespace Pulse.Core
{
    public static class EventHandlerExm
    {
        public static void NullSafeInvoke<T>(this EventHandler<T> self, object sender, T args)
        {
            EventHandler<T> h = self;
            if (h != null)
                h(sender, args);
        }
    }
}