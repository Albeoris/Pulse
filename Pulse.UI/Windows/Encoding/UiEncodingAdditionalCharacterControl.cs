using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Pulse.Core;

namespace Pulse.UI.Encoding
{
    public sealed class UiEncodingAdditionalCharacterControl : UiStackPanel
    {
        private readonly UiTextBlock _indexLabel;
        private readonly UiIntegerUpDown _rowNumber;
        private readonly UiIntegerUpDown _colNumber;
        private readonly UiWatermarkTextBox _output;
        private readonly UiWatermarkTextBox _input;
        
        private UiEncodingWindowSource _source;
        private int _index;

        private string _oldInputText = string.Empty;
        private int _largeIndex;

        public AutoResetEvent DrawEvent { get; set; }

        public UiEncodingAdditionalCharacterControl()
        {
            Orientation = Orientation.Horizontal;
            VerticalAlignment = VerticalAlignment.Center;

            #region Construct

            _indexLabel = UiTextBlockFactory.Create("#");
            {
                _indexLabel.Margin = new Thickness(5);
                _indexLabel.VerticalAlignment = VerticalAlignment.Center;
                Children.Add(_indexLabel);
            }

            UiTextBlock rowLabel = UiTextBlockFactory.Create(Lang.EncodingEditor.Extra.Row);
            {
                rowLabel.Margin = new Thickness(5, 5, 2, 5);
                rowLabel.VerticalAlignment = VerticalAlignment.Center;
                Children.Add(rowLabel);
            }

            _rowNumber = UiIntegerUpDownFactory.Create(0, byte.MaxValue);
            {
                _rowNumber.Width = 50;
                _rowNumber.Margin = new Thickness(2, 5, 5, 5);
                _rowNumber.ValueChanged += OnRowValueChanged;
                Children.Add(_rowNumber);
            }

            UiTextBlock colLabel = UiTextBlockFactory.Create(Lang.EncodingEditor.Extra.Column);
            {
                colLabel.Margin = new Thickness(5, 5, 2, 5);
                colLabel.VerticalAlignment = VerticalAlignment.Center;
                Children.Add(colLabel);
            }

            _colNumber = UiIntegerUpDownFactory.Create(0, byte.MaxValue);
            {
                _colNumber.Width = 50;
                _colNumber.Margin = new Thickness(2, 5, 5, 5);
                _colNumber.ValueChanged += OnColValueChanged;
                Children.Add(_colNumber);
            }

            UiTextBlock outputLabel = UiTextBlockFactory.Create(Lang.EncodingEditor.Extra.ToText);
            {
                outputLabel.Margin = new Thickness(5, 5, 2, 5);
                outputLabel.VerticalAlignment = VerticalAlignment.Center;
                Children.Add(outputLabel);
            }

            _output = UiWatermarkTextBoxFactory.Create("0x31->\"1\"");
            {
                _output.Width = 60;
                _output.Margin = new Thickness(2, 5, 5, 5);
                _output.TextChanged += OnOutputTextChanged;
                Children.Add(_output);
            }

            UiTextBlock inputLabel = UiTextBlockFactory.Create(Lang.EncodingEditor.Extra.FromText);
            {
                inputLabel.Margin = new Thickness(5, 5, 2, 5);
                inputLabel.VerticalAlignment = VerticalAlignment.Center;
                Children.Add(inputLabel);
            }

            _input = UiWatermarkTextBoxFactory.Create("0x31<-\"1\"");
            {
                _input.Width = 100;
                _input.Margin = new Thickness(2, 5, 5, 5);
                _input.TextChanged += OnInputTextChanged;
                Children.Add(_input);
            }

            #endregion
        }

        private void OnOutputTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_source == null || _index < 0)
                return;

            UiWatermarkTextBox box = (UiWatermarkTextBox)sender;
            string text = box.Text;
            if (text != null && text.Length > 1)
            {
                box.Text = text[text.Length - 1].ToString(CultureInfo.CurrentCulture);
                box.CaretIndex = text.Length;
                return;
            }

            char ch = string.IsNullOrEmpty(text) ? '\0' : text[0];
            _source.Chars[_largeIndex] = ch;
        }

        private void OnInputTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_source == null || _index < 0)
                return;

            foreach (char oldCh in _oldInputText)
                _source.Codes.Remove(oldCh);

            _oldInputText = string.Empty;

            UiWatermarkTextBox box = (UiWatermarkTextBox)sender;
            string text = box.Text ?? string.Empty;

            if (text.Length < 1)
                return;

            if (text.Length > 4)
            {
                box.Text = text.Remove(0, text.Length - 4);
                box.CaretIndex = text.Length;
                return;
            }

            foreach (char ch in text)
                _source.Codes[ch] = (short)_largeIndex;

            _oldInputText = text;
        }

        public void Load(UiEncodingWindowSource source, int index)
        {
            _source = null;
            _index = -1;
            _largeIndex = index + 256;

            _oldInputText = string.Empty;
            short value = source.Info.AdditionalTable[index];

            _indexLabel.Text = "0x" + (0x8140 + index).ToString("X");
            _rowNumber.Value = (value >> 8);
            _colNumber.Value = (value & 0xFF);

            _output.Text = source.Chars[_largeIndex].ToString(CultureInfo.CurrentCulture);
            _input.Text = String.Join(string.Empty, source.Codes.SelectWhere(p => p.Value == _largeIndex, p => p.Key));

            _source = source;
            _index = index;
        }

        private void OnRowValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_source == null)
                return;

            int? newValue = (int?)e.NewValue;
            if (newValue == null)
            {
                UiIntegerUpDown control = (UiIntegerUpDown)sender;
                control.Value = (int?)e.OldValue ?? (control.Minimum ?? 0);
                return;
            }

            _source.Info.AdditionalTable[_index] = (short)((_source.Info.AdditionalTable[_index]& 0xFF) | (((int)e.NewValue << 8) & 0x0000FF00));
            DrawEvent.NullSafeSet();
        }

        private void OnColValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_source == null)
                return;

            int? newValue = (int?)e.NewValue;
            if (newValue == null)
            {
                UiIntegerUpDown control = (UiIntegerUpDown)sender;
                control.Value = (int?)e.OldValue ?? (control.Minimum ?? 0);
                return;
            }

            _source.Info.AdditionalTable[_index] = (short)((_source.Info.AdditionalTable[_index] & 0xFF00) | ((int)e.NewValue & 0x000000FF));
            DrawEvent.NullSafeSet();
        }
    }
}