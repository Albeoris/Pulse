using System;

namespace Pulse.Core
{
    public sealed class DisposableAction : IDisposable
    {
        private readonly Action _action;
        private bool _canceled;

        public DisposableAction(Action action)
        {
            _action = Exceptions.CheckArgumentNull(action, "action");
        }

        public void Dispose()
        {
            if (!_canceled)
                _action();
        }

        public void Cancel()
        {
            _canceled = true;
        }
    }
}