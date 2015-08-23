using System;
using System.Threading;
using System.Windows.Input;
using Pulse.Core;

namespace Pulse.UI
{
    public sealed class UiDataProviderNodeRefreshCommand : ICommand
    {
        private int _canExecute = 1;
        private Action _action;

        public UiDataProviderNodeRefreshCommand(Action action)
        {
            _action = action;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == 1;
        }

        public void Execute(object parameter)
        {
            if (Interlocked.Exchange(ref _canExecute, 0) != 1) return;
            CanExecuteChanged.NullSafeInvoke(this, new EventArgs());

            try
            {
                _action.Invoke();
            }
            catch(Exception ex)
            {
                UiHelper.ShowError(null, ex);
            }

            Interlocked.Exchange(ref _canExecute, 1);
            CanExecuteChanged.NullSafeInvoke(this, new EventArgs());
        }

        public event EventHandler CanExecuteChanged;
    }
}