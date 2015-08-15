using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using Pulse.Core;
using Timer = System.Timers.Timer;

namespace Pulse.UI
{
    public sealed class UiProgressWindow : UiWindow, IDisposable
    {
        public UiProgressWindow(string title, UiProgressUnits units = UiProgressUnits.Items)
        {
            _units = units;

            #region Construct

            Height = 72;
            Width = 320;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            WindowStyle = WindowStyle.None;

            UiGrid root = UiGridFactory.Create(3, 1);
            root.SetRowsHeight(GridLength.Auto);
            root.Margin = new Thickness(5);

            UiTextBlock titleTextBlock = UiTextBlockFactory.Create(title);
            {
                titleTextBlock.VerticalAlignment = VerticalAlignment.Center;
                titleTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
                root.AddUiElement(titleTextBlock, 0, 0);
            }

            _progressBar = UiProgressBarFactory.Create();
            {
                root.AddUiElement(_progressBar, 1, 0);
            }

            _progressTextBlock = UiTextBlockFactory.Create("100%");
            {
                _progressTextBlock.VerticalAlignment = VerticalAlignment.Center;
                _progressTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
                root.AddUiElement(_progressTextBlock, 1, 0);
            }

            _elapsedTextBlock = UiTextBlockFactory.Create(Lang.Measurement.Elapsed + ": 00:00");
            {
                _elapsedTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
                root.AddUiElement(_elapsedTextBlock, 2, 0);
            }

            _processedTextBlock = UiTextBlockFactory.Create("0 / 0");
            {
                _processedTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
                root.AddUiElement(_processedTextBlock, 2, 0);
            }

            _remainingTextBlock = UiTextBlockFactory.Create(Lang.Measurement.Remaining + ": 00:00");
            {
                _remainingTextBlock.HorizontalAlignment = HorizontalAlignment.Right;
                root.AddUiElement(_remainingTextBlock, 2, 0);
            }

            Content = root;

            #endregion

            Loaded += OnLoaded;
            Closing += OnClosing;

            _timer = new Timer(500);
            _timer.Elapsed += OnTimer;
        }

        private readonly UiProgressBar _progressBar;
        private readonly UiTextBlock _progressTextBlock;
        private readonly UiTextBlock _elapsedTextBlock;
        private readonly UiTextBlock _processedTextBlock;
        private readonly UiTextBlock _remainingTextBlock;

        private readonly Timer _timer;

        private UiProgressUnits _units;
        private long _processedCount, _totalCount;
        private DateTime _begin;

        public void Dispose()
        {
            _timer.SafeDispose();
        }

        public void SetTotal(long totalCount)
        {
            Interlocked.Exchange(ref _totalCount, totalCount);
        }

        public void Incremented(long processedCount)
        {
            if (Interlocked.Add(ref _processedCount, processedCount) < 0)
                throw new ArgumentOutOfRangeException(nameof(processedCount));
        }

        #region Internal Logic

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _begin = DateTime.Now;
            _timer.Start();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            _timer.Stop();
            _timer.Elapsed -= OnTimer;
        }

        private void OnTimer(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            Dispatcher.Invoke(DispatcherPriority.DataBind, (Action)(UpdateProgress));
        }

        private void UpdateProgress()
        {
            _timer.Elapsed -= OnTimer;

            _progressBar.Maximum = _totalCount;
            _progressBar.Value = _processedCount;

            double percents = (_totalCount == 0) ? 0.0 : 100 * _processedCount / (double)_totalCount;
            TimeSpan elapsed = DateTime.Now - _begin;
            double speed = _processedCount / Math.Max(elapsed.TotalSeconds, 1);
            if (speed < 1) speed = 1;
            TimeSpan left = TimeSpan.FromSeconds((_totalCount - _processedCount) / speed);

            _progressTextBlock.Text = $"{percents:F2}%";
            _elapsedTextBlock.Text = String.Format("{1}: {0:mm\\:ss}", elapsed, Lang.Measurement.Elapsed);
            _processedTextBlock.Text = $"{FormatValue(_processedCount)} / {FormatValue(_totalCount)}";
            _remainingTextBlock.Text = String.Format("{1}: {0:mm\\:ss}", left, Lang.Measurement.Remaining);

            _timer.Elapsed += OnTimer;
        }

        private string FormatValue(long value)
        {
            return _units == UiProgressUnits.Items ? value.ToString(CultureInfo.CurrentCulture) : UiHelper.FormatBytes(value);
        }

        #endregion

        public static void Execute(string title, IProgressSender progressSender, Action action, UiProgressUnits units = UiProgressUnits.Items)
        {
            using (UiProgressWindow window = new UiProgressWindow(title, units))
            {
                progressSender.ProgressTotalChanged += window.SetTotal;
                progressSender.ProgressIncremented += window.Incremented;
                Task.Run(() => ExecuteAction(window, action));
                window.ShowDialog();
            }
        }

        public static T Execute<T>(string title, IProgressSender progressSender, Func<T> func, UiProgressUnits units = UiProgressUnits.Items)
        {
            using (UiProgressWindow window = new UiProgressWindow(title, units))
            {
                progressSender.ProgressTotalChanged += window.SetTotal;
                progressSender.ProgressIncremented += window.Incremented;
                Task<T> task = Task.Run(() => ExecuteFunction(window, func));
                window.ShowDialog();
                return task.Result;
            }
        }

        public static void Execute(string title, Action<Action<long>, Action<long>> action, UiProgressUnits units = UiProgressUnits.Items)
        {
            using (UiProgressWindow window = new UiProgressWindow(title, units))
            {
                Task.Run(() => ExecuteAction(window, action));
                window.ShowDialog();
            }
        }

        public static T Execute<T>(string title, Func<Action<long>, Action<long>, T> action, UiProgressUnits units = UiProgressUnits.Items)
        {
            using (UiProgressWindow window = new UiProgressWindow(title, units))
            {
                Task<T> task = Task.Run(() => ExecuteFunction(window, action));
                window.ShowDialog();
                return task.Result;
            }
        }

        #region Internal Static Logic

        private static void ExecuteAction(UiProgressWindow window, Action action)
        {
            try
            {
                action();
            }
            finally
            {
                window.Dispatcher.Invoke(window.Close);
            }
        }

        private static void ExecuteAction(UiProgressWindow window, Action<Action<long>, Action<long>> action)
        {
            try
            {
                action(window.SetTotal, window.Incremented);
            }
            finally
            {
                window.Dispatcher.Invoke(window.Close);
            }
        }

        private static T ExecuteFunction<T>(UiProgressWindow window, Func<T> func)
        {
            try
            {
                return func();
            }
            finally
            {
                window.Dispatcher.Invoke(window.Close);
            }
        }

        private static T ExecuteFunction<T>(UiProgressWindow window, Func<Action<long>, Action<long>, T> action)
        {
            try
            {
                return action(window.SetTotal, window.Incremented);
            }
            finally
            {
                window.Dispatcher.Invoke(window.Close);
            }
        }

        #endregion
    }
}