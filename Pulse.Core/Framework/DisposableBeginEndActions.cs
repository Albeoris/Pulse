using System;

namespace Pulse.Core
{
    public sealed class DisposableBeginEndActions : IDisposable
    {
        private readonly Action _end;

        public DisposableBeginEndActions(Action begin, Action end)
        {
            Exceptions.CheckArgumentNull(begin, "begin").Invoke();
            _end = Exceptions.CheckArgumentNull(end, "end");
        }

        public void Dispose()
        {
            _end();
        }
    }

    public sealed class DisposableBeginEndActions<T> : IDisposable
    {
        private readonly T _beginResult;
        private readonly Action<T> _end;

        public DisposableBeginEndActions(Func<T> begin, Action<T> end)
        {
            _beginResult = Exceptions.CheckArgumentNull(begin, "begin").Invoke();
            _end = Exceptions.CheckArgumentNull(end, "end");
        }

        public void Dispose()
        {
            _end(_beginResult);
        }
    }
}