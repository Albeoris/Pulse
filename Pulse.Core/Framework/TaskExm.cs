using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pulse.Core
{
    public static class TaskExm
    {
        public static void CancelAndWait(this Task self, CancellationTokenSource cts, int millisecondsTimeout)
        {
            cts.Cancel();
            if (!self.Wait(millisecondsTimeout))
                throw new TimeoutException();
        }
    }
}