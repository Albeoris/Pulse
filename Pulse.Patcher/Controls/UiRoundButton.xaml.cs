using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Pulse.Core;
using Pulse.UI;

namespace Pulse.Patcher
{
    public abstract partial class UiRoundButton
    {
        private readonly ManualResetEvent _workingEvent = new ManualResetEvent(false);
        protected readonly ManualResetEvent CancelEvent = new ManualResetEvent(false);

        protected UiRoundButton()
        {
            Loaded += OnLoaded;
            Dispatcher.ShutdownStarted += OnDispatcherShutdownStarted;

            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            PatcherService.RegisterControl(this);
        }

        private void OnDispatcherShutdownStarted(object sender, EventArgs e)
        {
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
                    await DoAction();
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
            }
            finally
            {
                PatcherService.ChangeEnableState(this, true);
            }
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label", typeof(string), typeof(UiRoundButton), new PropertyMetadata("Button Label"));
        public static readonly DependencyProperty SubLabelProperty = DependencyProperty.Register("SubLabel", typeof(string), typeof(UiRoundButton), new PropertyMetadata(null));
        public static readonly DependencyProperty SubLabelColorProperty = DependencyProperty.Register("SubLabelColor", typeof(Brush), typeof(UiRoundButton), new PropertyMetadata(Brushes.DarkGray));

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
    }
}