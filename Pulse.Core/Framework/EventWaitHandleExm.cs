using System.Threading;

namespace Pulse.Core
{
    public static class EventWaitHandleExm
    {
        public static void NullSafeSet(this EventWaitHandle handle)
        {
            handle?.Set();
        }
    }
}