using System;

namespace Pulse.Core
{
    public sealed class ProgressSender : IProgressSender
    {
        public event Action<long> ProgressTotalChanged;
        public event Action<long> ProgressIncremented;

        public void IncrementProgress(long value)
        {
            ProgressIncremented.NullSafeInvoke(value);
        }

        public void ChangeTotal(long value)
        {
            ProgressTotalChanged.NullSafeInvoke(value);
        }
    }
}