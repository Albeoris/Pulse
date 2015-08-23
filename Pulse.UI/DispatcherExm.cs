using System;
using System.Windows.Threading;

namespace Pulse.UI
{
    public static class DispatcherExm
    {
        public static void NecessityInvoke(this Dispatcher self, Action action)
        {
            if (self.CheckAccess())
                action();

            self.Invoke(action);
        }
    }
}