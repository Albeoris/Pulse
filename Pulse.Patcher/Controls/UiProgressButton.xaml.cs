using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Pulse.Core;
using Pulse.UI;

namespace Pulse.Patcher
{
    public abstract partial class UiProgressButton
    {
        private TimeoutAction _progressThread;
        private long _position;
        private long _maximum;

        private readonly ManualResetEvent _workingEvent = new ManualResetEvent(false);
        protected readonly ManualResetEvent CancelEvent = new ManualResetEvent(false);

        protected UiProgressButton()
        {
            Loaded += OnLoaded;
            Dispatcher.ShutdownStarted += OnDispatcherShutdownStarted;

            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _progressThread = new TimeoutAction(DrawProgressBar, 25);
            PatcherService.RegisterControl(this);
        }

        private void OnDispatcherShutdownStarted(object sender, EventArgs e)
        {
            _progressThread.Dispose();
            _progressThread = null;
            PatcherService.UnregisterControl(this);
        }

        protected abstract Task DoAction();

        protected override async void OnClick()
        {
            try
            {
                PatcherService.ChangeEnableState(this, false);
                if (_workingEvent.WaitOne(0) && !CancelEvent.WaitOne(0))
                {
                    Label = "Отмена...";
                    CancelEvent.Set();
                    return;
                }

                string label = Label;
                try
                {
                    BeginProcess();
                    await DoAction();

                    if (CancelEvent.WaitOne(0))
                        EndProcessError();
                    else
                        EndProcessSuccess();
                }
                finally
                {
                    _workingEvent.Reset();
                    CancelEvent.Reset();
                    Label = label;
                }
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(this, ex);
                EndProcessError();
            }
            finally
            {
                PatcherService.ChangeEnableState(this, true);
            }
        }

        public static readonly DependencyProperty GradientLeftOffsetProperty = DependencyProperty.Register("GradientLeftOffset", typeof(double), typeof(UiProgressButton), new PropertyMetadata(0.0d));
        public static readonly DependencyProperty GradientRightOffsetProperty = DependencyProperty.Register("GradientRightOffset", typeof(double), typeof(UiProgressButton), new PropertyMetadata(0.0d));
        public static readonly DependencyProperty RedGreenRectVisibilityProperty = DependencyProperty.Register("RedGreenRectVisibility", typeof(Visibility), typeof(UiProgressButton), new PropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty BlueRectVisibilityProperty = DependencyProperty.Register("BlueRectVisibility", typeof(Visibility), typeof(UiProgressButton), new PropertyMetadata(Visibility.Hidden));
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label", typeof(string), typeof(UiProgressButton), new PropertyMetadata("Button Label"));
        public static readonly DependencyProperty SubLabelProperty = DependencyProperty.Register("SubLabel", typeof(string), typeof(UiProgressButton), new PropertyMetadata(null));
        public static readonly DependencyProperty SubLabelColorProperty = DependencyProperty.Register("SubLabelColor", typeof(Brush), typeof(UiProgressButton), new PropertyMetadata(Brushes.DarkGray));

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public string SubLabel
        {
            get { return (string)GetValue(SubLabelProperty); }
            set { SetValue(SubLabelProperty, value); }
        }

        public Brush SubLabelColor
        {
            get { return (Brush)GetValue(SubLabelColorProperty); }
            set { SetValue(SubLabelColorProperty, value); }
        }

        public void BeginProcess()
        {
            _workingEvent.Set();

            Position = 0;
            Maximum = 0;

            SetValue(BlueRectVisibilityProperty, Visibility.Hidden);
            SetValue(RedGreenRectVisibilityProperty, Visibility.Visible);
        }

        public void EndProcessSuccess()
        {
            SetValue(BlueRectVisibilityProperty, Visibility.Visible);
            SetValue(RedGreenRectVisibilityProperty, Visibility.Hidden);
        }

        private void EndProcessError()
        {
            Position = 0;
            Maximum = 0;
        }

        public long Position
        {
            get { return Interlocked.Read(ref _position); }
            set
            {
                Interlocked.Exchange(ref _position, value);
                _progressThread.WorkEvent.Set();
            }
        }

        public long Maximum
        {
            get { return Interlocked.Read(ref _maximum); }
            set
            {
                Interlocked.Exchange(ref _maximum, value);
                _progressThread.WorkEvent.Set();
            }
        }

        private void DrawProgressBar()
        {
            double maximum = Math.Max(0.0d, _maximum);

            if (maximum < 1)
            {
                ChangeGradientOffset(0.0d);
            }
            else
            {
                double position = Math.Min(Math.Max(0, _position), maximum);
                double offset = position / maximum;
                ChangeGradientOffset(offset);
            }
        }

        private void ChangeGradientOffset(double offset)
        {
            try
            {
                if (CheckAccess())
                {
                    SetValue(GradientLeftOffsetProperty, offset);
                    SetValue(GradientRightOffsetProperty, offset + 0.01);
                }
                else
                {
                    Dispatcher.Invoke(() => ChangeGradientOffset(offset));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}