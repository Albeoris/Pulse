using System;
using System.Windows;
using System.Windows.Controls;

namespace Pulse.UI.Encoding
{
    public sealed class UiEncodingLabeledNumber : UiGrid
    {
        public readonly UiIntegerUpDown NumberControl;

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

            NumberControl = UiIntegerUpDownFactory.Create(minValue, maxValue);
            {
                NumberControl.Width = width;
                NumberControl.Margin = new Thickness(2, 5, 5, 5);
                NumberControl.ValueChanged += OnValueChanged;
                AddUiElement(NumberControl, 0, 1);
            }
        }

        public int? Value
        {
            get { return NumberControl.Value; }
            set { NumberControl.Value = value; }
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

            _onValueChanged?.Invoke(this, e);
        }
    }
}