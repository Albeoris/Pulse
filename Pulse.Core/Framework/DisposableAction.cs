using System;

namespace Pulse.Core
{
    public sealed class DisposableAction : IDisposable
    {
        private readonly Action _action;
        private bool _isCanceled;
        private bool _isSafe;

        public DisposableAction(Action action, bool isSafe = false)
        {
            _action = Exceptions.CheckArgumentNull(action, "action");
            _isSafe = isSafe;
        }

        public void Dispose()
        {
            try
            {
                if (!_isCanceled)
                    _action();
            }
            catch (Exception ex)
            {
                if (_isSafe)
                    Log.Error(ex);
                else
                    throw;
            }
        }

        public void Cancel()
        {
            _isCanceled = true;
        }
    }
}