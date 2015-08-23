using System.Threading;

namespace Pulse.Core
{
    public static class ManualResetEventExm
    {
        public static bool IsSet(this ManualResetEvent handle)
        {
            return handle != null && handle.WaitOne(0);
        }
    }
}