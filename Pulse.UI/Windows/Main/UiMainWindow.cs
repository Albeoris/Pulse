
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using Pulse.Core;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using Timer = System.Timers.Timer;

namespace Pulse.UI
{
    public sealed class UiMainWindow : UiWindow, IDisposable
    {
        public UiMainWindow()
        {
            #region Construct

            Assembly assembly = Assembly.GetEntryAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            Title = String.Format("{0} {1} {2}", fvi.ProductName, fvi.FileVersion, fvi.LegalCopyright);

            Width = 640;
            Height = 480;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            WindowState = WindowState.Maximized;

            UiGrid root = UiGridFactory.Create(2, 1);
            root.RowDefinitions[0].Height = GridLength.Auto;

            DockingManager dockingManager = new DockingManager();
            {
                root.AddUiElement(dockingManager, 1, 0);
            }

            UiMenu mainMenu = UiMenuFactory.Create();
            {
                UiMenuItem viewItem = mainMenu.AddChild(UiMenuItemFactory.Create("Вид"));
                {
                    foreach (UiMainDockableControl dockable in UiMainDockableControl.CreateKnownDockables(dockingManager))
                        viewItem.AddChild(dockable.CreateMenuItem());
                }
                
                root.AddUiElement(mainMenu, 0, 0);
            }

            

            //_progressBar = UiProgressBarFactory.Create();
            //{
            //    root.AddUiElement(_progressBar, 1, 0);
            //}

            //_progressTextBlock = UiTextBlockFactory.Create("100%");
            //{
            //    _progressTextBlock.VerticalAlignment = VerticalAlignment.Center;
            //    _progressTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
            //    root.AddUiElement(_progressTextBlock, 1, 0);
            //}

            //_elapsedTextBlock = UiTextBlockFactory.Create("Прошло: 00:00");
            //{
            //    _elapsedTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
            //    root.AddUiElement(_elapsedTextBlock, 2, 0);
            //}

            //_processedTextBlock = UiTextBlockFactory.Create("0 / 0");
            //{
            //    _processedTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
            //    root.AddUiElement(_processedTextBlock, 2, 0);
            //}

            //_remainingTextBlock = UiTextBlockFactory.Create("Осталось: 00:00");
            //{
            //    _remainingTextBlock.HorizontalAlignment = HorizontalAlignment.Right;
            //    root.AddUiElement(_remainingTextBlock, 2, 0);
            //}

            Content = root;

            #endregion

            //Loaded += OnLoaded;
            //Closing += OnClosing;
            //
            //_timer = new Timer(500);
            //_timer.Elapsed += OnTimer;

            InteractionService.Refresh();
        }

        private readonly UiProgressBar _progressBar;
        private readonly UiTextBlock _progressTextBlock;
        private readonly UiTextBlock _elapsedTextBlock;
        private readonly UiTextBlock _processedTextBlock;
        private readonly UiTextBlock _remainingTextBlock;

        private readonly Timer _timer;

        private long _processedCount, _totalCount;
        private DateTime _begin;

        public void Dispose()
        {
            _timer.SafeDispose();
        }

        public void SetTotal(long totalCount)
        {
            Interlocked.Add(ref _totalCount, totalCount);
        }

        public void Increment(long processedCount)
        {
            Interlocked.Add(ref _processedCount, processedCount);
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

            _progressTextBlock.Text = String.Format("{0:F2}%", percents);
            _elapsedTextBlock.Text = String.Format("{1}: {0:mm\\:ss}", elapsed, "Прошло");
            _processedTextBlock.Text = String.Format("{0} / {1}", _processedCount, _totalCount);
            _remainingTextBlock.Text = String.Format("{1}: {0:mm\\:ss}", left, "Осталось");

            _timer.Elapsed += OnTimer;
        }

        #endregion

        public static void Execute(string title, IProgressSender progressSender, Action action)
        {
            using (UiProgressWindow window = new UiProgressWindow(title))
            {
                progressSender.ProgressTotalChanged += window.SetTotal;
                progressSender.ProgressIncrement += window.Increment;
                Task.Run(() => ExecuteAction(window, action));
                window.ShowDialog();
            }
        }

        public static T Execute<T>(string title, IProgressSender progressSender, Func<T> func)
        {
            using (UiProgressWindow window = new UiProgressWindow(title))
            {
                progressSender.ProgressTotalChanged += window.SetTotal;
                progressSender.ProgressIncrement += window.Increment;
                Task<T> task = Task.Run(() => ExecuteFunction(window, func));
                window.ShowDialog();
                return task.Result;
            }
        }

        public static void Execute(string title, Action<Action<long>, Action<long>> action)
        {
            using (UiProgressWindow window = new UiProgressWindow(title))
            {
                Task.Run(() => ExecuteAction(window, action));
                window.ShowDialog();
            }
        }

        public static T Execute<T>(string title, Func<Action<long>, Action<long>, T> action)
        {
            using (UiProgressWindow window = new UiProgressWindow(title))
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
                action(window.SetTotal, window.Increment);
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
                return action(window.SetTotal, window.Increment);
            }
            finally
            {
                window.Dispatcher.Invoke(window.Close);
            }
        }

        #endregion
    }
}