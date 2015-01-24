using System;
using System.Windows;
using System.Windows.Controls;

namespace Pulse.UI.Encoding
{
    public sealed class UiEncodingLabeledNumber : UiGrid
    {
        private readonly UiIntegerUpDown _numberControl;
        private readonly RoutedPropertyChangedEventHandler<object> _onValueChanged;

        public UiEncodingLabeledNumber(string label, int width, int minValue, int maxValue, RoutedPropertyChangedEventHandler<object> onValueChanged)
        {
            _onValueChanged = onValueChanged;

            ColumnDefinitions.Add(new ColumnDefinition() {Width = GridLength.Auto});
            ColumnDefinitions.Add(new ColumnDefinition());

            Margin = new Thickness(5);

            UiTextBlock labelControl = UiTextBlockFactory.Create(label);
            {
                labelControl.Margin = new Thickness(5, 5, 2, 5);
                labelControl.VerticalAlignment = VerticalAlignment.Center;
                AddUiElement(labelControl, 0, 0);
            }

            _numberControl = UiIntegerUpDownFactory.Create(minValue, maxValue);
            {
                _numberControl.Width = width;
                _numberControl.Margin = new Thickness(2, 5, 5, 5);
                _numberControl.ValueChanged += OnValueChanged;
                AddUiElement(_numberControl, 0, 1);
            }
        }

        public int? Value
        {
            get { return _numberControl.Value; }
            set { _numberControl.Value = value; }
        }

        private void OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            int? newValue = (int?)e.NewValue;
            if (newValue == null)
            {
                UiIntegerUpDown control = (UiIntegerUpDown)sender;
                control.Value = (int?)e.OldValue ?? Math.Max((control.Minimum ?? 0), 0);
                return;
            }

            if (_onValueChanged != null)
                _onValueChanged.Invoke(this, e);
        }
    }
}