using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Pulse.Patcher.Annotations;
using Pulse.UI;

namespace Pulse.Patcher.Controls
{
    public sealed class GameSettingsControl : UiGrid, INotifyPropertyChanged
    {
        private LocalizatorEnvironmentInfo _info;

        public GameSettingsControl()
        {
            SetCols(2);
            SetRows(9);

            Width = 250;
            VerticalAlignment = VerticalAlignment.Top;
            HorizontalAlignment = HorizontalAlignment.Left;
            Margin = new Thickness(5);
            DataContext = this;

            _info = InteractionService.LocalizatorEnvironment.Provide();
            InteractionService.LocalizatorEnvironment.InfoProvided += OnLocalizatorEnvironmentProvided;

            LinearGradientBrush backgroundStroke = new LinearGradientBrush
            {
                EndPoint = new Point(0.5, 1),
                StartPoint = new Point(0.5, 0),
                RelativeTransform = new RotateTransform(115, 0.5, 0.5),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(0xff, 0x61, 0x61, 0x61), 0),
                    new GradientStop(Color.FromArgb(0xff, 0xF2, 0xF2, 0xF2), 0.504),
                    new GradientStop(Color.FromArgb(0xff, 0xAE, 0xB1, 0xB1), 1)
                }
            };
            backgroundStroke.Freeze();

            LinearGradientBrush backgroundFill = new LinearGradientBrush
            {
                MappingMode = BrushMappingMode.RelativeToBoundingBox,
                StartPoint = new Point(0.5, 1.0),
                EndPoint = new Point(0.5, -0.4),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(0xBB, 0x44, 0x71, 0xc1), 0),
                    new GradientStop(Color.FromArgb(0xBB, 0x28, 0x36, 0x65), 1)
                }
            };
            backgroundFill.Freeze();

            Rectangle backround = AddUiElement(new Rectangle {Stroke = backgroundStroke, Fill = backgroundFill, StrokeThickness = 5}, 0, 0, 9, 2);

            InverseBoolConverter inverseBoolConverter = new InverseBoolConverter();
            Thickness rowMargin = new Thickness(0, 8, 0, 3);

            const string screenGroup = "Отображение:";
            AddUiElement(UiTextBlockFactory.Create(screenGroup), 0, 0, 0, 2).Margin = rowMargin;
            AddUiElement(UiRadioButtonFactory.Create(screenGroup, "Экран", true), 1, 0).SetBinding(ToggleButton.IsCheckedProperty, new Binding("IsFullScreen") {Mode = BindingMode.TwoWay});
            AddUiElement(UiRadioButtonFactory.Create(screenGroup, "Окно", false), 1, 1).SetBinding(ToggleButton.IsCheckedProperty, new Binding("IsFullScreen") {Mode = BindingMode.TwoWay, Converter = inverseBoolConverter});

            const string resolutionGroup = "Разрешение:";
            AddUiElement(UiTextBlockFactory.Create(resolutionGroup), 2, 0, 0, 2).Margin = rowMargin;
            AddUiElement(UiRadioButtonFactory.Create(resolutionGroup, "1920x1080", true), 3, 0).SetBinding(ToggleButton.IsCheckedProperty, new Binding("IsFullHd") {Mode = BindingMode.TwoWay});
            AddUiElement(UiRadioButtonFactory.Create(resolutionGroup, "1280x720", false), 3, 1).SetBinding(ToggleButton.IsCheckedProperty, new Binding("IsFullHd") {Mode = BindingMode.TwoWay, Converter = inverseBoolConverter});

            const string voiceGroup = "Озвучка:";
            AddUiElement(UiTextBlockFactory.Create(voiceGroup), 4, 0, 0, 2).Margin = rowMargin;
            AddUiElement(UiRadioButtonFactory.Create(voiceGroup, "Японская", true), 5, 0).SetBinding(ToggleButton.IsCheckedProperty, new Binding("IsNihonVoice") {Mode = BindingMode.TwoWay});
            AddUiElement(UiRadioButtonFactory.Create(voiceGroup, "Английская", false), 5, 1).SetBinding(ToggleButton.IsCheckedProperty, new Binding("IsNihonVoice") {Mode = BindingMode.TwoWay, Converter = inverseBoolConverter});

            UiCheckBox switchButtons = AddUiElement(UiCheckBoxFactory.Create("Поменять кнопки X/O", null), 6, 0, 0, 2);
            switchButtons.Margin = rowMargin;
            switchButtons.IsThreeState = true;
            switchButtons.SetBinding(ToggleButton.IsCheckedProperty, new Binding("SwitchButtons") {Mode = BindingMode.TwoWay});

            AddUiElement(UiTextBlockFactory.Create("MSAA:"), 7, 0).Margin = rowMargin;
            UiComboBox antiAliasing = AddUiElement(UiComboBoxFactory.Create(), 8, 0);
            antiAliasing.ItemStringFormat = "x{0}";
            antiAliasing.ItemsSource = new[] {2, 4, 8, 16};
            antiAliasing.SelectedIndex = 3;
            antiAliasing.SetBinding(Selector.SelectedItemProperty, new Binding("AntiAliasing") {Mode = BindingMode.TwoWay});
            antiAliasing.Margin = new Thickness(0, 0, 0, 8);

            AddUiElement(UiTextBlockFactory.Create("Тени:"), 7, 1).Margin = rowMargin;
            UiComboBox shadows = AddUiElement(UiComboBoxFactory.Create(), 8, 1);
            shadows.ItemStringFormat = "{0}x{0}";
            shadows.ItemsSource = new[] {512, 1024, 2048, 4096, 8192};
            shadows.SelectedIndex = 1;
            shadows.SetBinding(Selector.SelectedItemProperty, new Binding("ShadowResolution") {Mode = BindingMode.TwoWay});
            shadows.Margin = new Thickness(0, 0, 0, 8);

            foreach (FrameworkElement child in Children)
            {
                if (!ReferenceEquals(child, backround))
                    child.Margin = new Thickness(child.Margin.Left + 8, child.Margin.Top, child.Margin.Right + 8, child.Margin.Bottom);

                TextBlock textblock = child as TextBlock;
                if (textblock != null)
                {
                    textblock.Foreground = Brushes.WhiteSmoke;
                    textblock.FontWeight = FontWeight.FromOpenTypeWeight(500);
                    continue;
                }

                Control control = child as Control;
                if (control != null && !(control is ComboBox))
                    control.Foreground = Brushes.WhiteSmoke;
            }
        }

        private void OnLocalizatorEnvironmentProvided(LocalizatorEnvironmentInfo obj)
        {
            _info = obj;
            
            OnPropertyChanged(nameof(IsFullScreen));
            OnPropertyChanged(nameof(IsFullHd));
            OnPropertyChanged(nameof(IsNihonVoice));
            OnPropertyChanged(nameof(SwitchButtons));
            OnPropertyChanged(nameof(AntiAliasing));
            OnPropertyChanged(nameof(ShadowResolution));
        }

        #region Properties

        public bool? IsFullScreen
        {
            get { return _info.IsFullScreen; }
            set
            {
                if (_info.IsFullScreen != value)
                {
                    _info.IsFullScreen = value == true;
                    OnPropertyChanged();
                }
            }
        }

        public bool? IsFullHd
        {
            get { return _info.IsFullHd; }
            set
            {
                if (_info.IsFullHd != value)
                {
                    _info.IsFullHd = value == true;
                    OnPropertyChanged();
                }
            }
        }

        public bool? IsNihonVoice
        {
            get { return _info.IsNihonVoice; }
            set
            {
                if (_info.IsNihonVoice != value)
                {
                    _info.IsNihonVoice = value == true;
                    OnPropertyChanged();
                }
            }
        }

        public bool? SwitchButtons
        {
            get { return _info.SwitchButtons; }
            set
            {
                if (_info.SwitchButtons != value)
                {
                    _info.SwitchButtons = value;
                    OnPropertyChanged();
                }
            }
        }

        public int AntiAliasing
        {
            get { return _info.AntiAliasing; }
            set
            {
                if (_info.AntiAliasing != value)
                {
                    _info.AntiAliasing = value;
                    OnPropertyChanged();
                }
            }
        }

        public int ShadowResolution
        {
            get { return _info.ShadowResolution; }
            set
            {
                if (_info.ShadowResolution != value)
                {
                    _info.ShadowResolution = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        public string GetGameProcessArguments()
        {
            StringBuilder sb = new StringBuilder(512);

            int width, height;
            if (IsFullScreen == true)
            {
                sb.Append("-FullScreenMode=Force ");
                width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
                height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            }
            else
            {
                if (IsFullHd == true)
                {
                    width = 1920;
                    height = 1080;
                }
                else
                {
                    width = 1280;
                    height = 720;
                }
            }
            sb.AppendFormat(CultureInfo.InvariantCulture, "-Width={0} -Height={1} ", width, height);

            if (IsNihonVoice == true)
                sb.Append("-VoiceJPMode ");

            sb.AppendFormat("-Shadow={0} ", ShadowResolution);
            sb.AppendFormat("-MSAA={0} ", AntiAliasing);

            if (SwitchButtons == true || (SwitchButtons == null && IsNihonVoice == true))
                sb.Append("-DecideButtonReverse ");

            return sb.ToString();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            InteractionService.Configuration.Provide().ScheduleSave();
        }

        #endregion
    }

    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool?)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool?)value;
        }
    }
}