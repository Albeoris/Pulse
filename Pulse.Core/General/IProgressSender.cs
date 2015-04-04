using System;

namespace Pulse.Core
{
    public interface IProgressSender
    {
        event Action<long> ProgressTotalChanged;
        event Action<long> ProgressIncremented;
    }
}