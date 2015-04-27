using System;
using System.IO;
using System.Threading;

namespace Pulse.Core
{
    public sealed class TimeoutAction : IDisposable
    {
        private readonly Action _action;
        private readonly Thread _thread;
        private readonly int _millisecondsTimeout;

        public event Action<Exception> ErrorHandled;

        public readonly AutoResetEvent WorkEvent = new AutoResetEvent(false);

        public TimeoutAction(Action action, int millisecondsTimeout)
        {
            _action = action;
            _millisecondsTimeout = millisecondsTimeout;
            _thread = new Thread(OnTick) {IsBackground = true, Name = GetThreadName()};
            _thread.Start();
        }

        public void Dispose()
        {
            try
            {
                _thread.Abort();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private void OnTick()
        {
            while (true)
            {
                try
                {
                    WorkEvent.WaitOne();
                    DateTime begin = DateTime.Now;

                    _action();

                    int timeout = Math.Max(0, _millisecondsTimeout - (int)(DateTime.Now - begin).TotalMilliseconds);
                    Thread.Sleep(timeout);
                }
                catch (Exception ex)
                {
                    ErrorHandled.NullSafeInvoke(ex);
                }
            }
        }

        private static string GetThreadName()
        {
            try
            {
                using (StringReader reader = new StringReader(Environment.StackTrace))
                {
                    reader.ReadLine();
                    reader.ReadLine();
                    reader.ReadLine();
                    reader.ReadLine();
                    return "TimeoutAction: " + reader.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex);
                return "TimeoutAction";
            }
        }
    }
}