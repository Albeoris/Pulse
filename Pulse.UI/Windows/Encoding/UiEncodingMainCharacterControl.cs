using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Pulse.Core;

namespace Pulse.UI.Encoding
{
    public sealed class UiEncodingMainCharacterControl : UiStackPanel
    {
        private readonly UiTextBlock _indexLabel;
        private readonly UiEncodingLabeledNumber _oy;
        private readonly UiEncodingLabeledNumber _ox;
        private readonly UiEncodingLabeledNumber _before;
        private readonly UiEncodingLabeledNumber _width;
        private readonly UiEncodingLabeledNumber _after;
        private readonly UiEncodingLabeledWatermark _output;
        private readonly UiEncodingLabeledWatermark _input;

        private UiEncodingWindowSource _source;
        private int _index;
        private int _littleIndex;

        private string _oldInputText = string.Empty;

        public AutoResetEvent DrawEvent { get; set; }

        public UiEncodingMainCharacterControl()
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

            _ox = AddUiElement(new UiEncodingLabeledNumber("OX:", 60, 0, short.MaxValue, OXChanged));
            _oy = AddUiElement(new UiEncodingLabeledNumber("OY:", 60, 0, short.MaxValue, OYChanged));
            _before = AddUiElement(new UiEncodingLabeledNumber(Lang.EncodingEditor.Main.Before, 50, sbyte.MinValue, sbyte.MaxValue, BeforeChanged));
            _width = AddUiElement(new UiEncodingLabeledNumber(Lang.EncodingEditor.Main.Width, 50, 0, sbyte.MaxValue, WidthChanged));
            _after = AddUiElement(new UiEncodingLabeledNumber(Lang.EncodingEditor.Main.After, 50, sbyte.MinValue, sbyte.MaxValue, AfterChanged));
            _output = AddUiElement(new UiEncodingLabeledWatermark(Lang.EncodingEditor.Main.ToText, "0x31->\"1\"", 50, OnOutputTextChanged));
            _input = AddUiElement(new UiEncodingLabeledWatermark(Lang.EncodingEditor.Main.FromText, "0x31<-\"1\"", 70, OnInputTextChanged));

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
            _source.Chars[_littleIndex] = ch;
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
                _source.Codes[ch] = (short)_littleIndex;

            _oldInputText = text;
        }

        public void Load(UiEncodingWindowSource source, int index)
        {
            _source = null;
            _index = -1;

            _oldInputText = string.Empty;
            _littleIndex = index % 256;

            int offsets = source.Info.Offsets[index];
            int sizes = source.Info.Sizes[index];

            int oy = (offsets >> 16) & 0xFFFF;
            int ox = offsets & 0xFFFF;

            sbyte before = (sbyte)(sizes & 0x000000FF);
            sbyte width = (sbyte)((sizes & 0x0000FF00) >> 8);
            sbyte after = (sbyte)((sizes & 0x00FF0000) >> 16);

            _indexLabel.Text = "0x" + (index).ToString("X");
            _ox.Value = ox;
            _oy.Value = oy;
            _before.Value = before;
            _width.Value = width;
            _after.Value = after;

            _output.Text = source.Chars[_littleIndex].ToString(CultureInfo.CurrentCulture);
            _input.Text = _oldInputText = String.Join(string.Empty, source.Codes.SelectWhere(p => p.Value == _littleIndex, p => p.Key));

            _source = source;
            _index = index;
        }

        private void OXChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_source == null)
                return;

            int newValue = (int)e.NewValue;

            int x, y;
            _source.Info.GetOffsets(_index, out x, out y);
            _source.Info.SetOffsets(_index, newValue, y);

            DrawEvent.NullSafeSet();
        }

        private void OYChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_source == null)
                return;

            int newValue = (int)e.NewValue;

            int x, y;
            _source.Info.GetOffsets(_index, out x, out y);
            _source.Info.SetOffsets(_index, x, newValue);

            DrawEvent.NullSafeSet();
        }

        private void BeforeChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_source == null)
                return;

            sbyte newValue = (sbyte)(int)e.NewValue;

            byte before, width, after;
            _source.Info.GetSizes(_index, out before, out width, out after);
            _source.Info.SetSizes(_index, (byte)newValue, width, after);

            DrawEvent.NullSafeSet();
        }

        private void WidthChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_source == null)
                return;

            sbyte newValue = (sbyte)(int)e.NewValue;

            byte before, width, after;
            _source.Info.GetSizes(_index, out before, out width, out after);
            _source.Info.SetSizes(_index, before, (byte)newValue, after);

            DrawEvent.NullSafeSet();
        }

        private void AfterChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_source == null)
                return;

            sbyte newValue = (sbyte)(int)e.NewValue;

            byte before, width, after;
            _source.Info.GetSizes(_index, out before, out width, out after);
            _source.Info.SetSizes(_index, before, width, (byte)newValue);

            DrawEvent.NullSafeSet();
        }

        public void IncrementXY(int ox, int oy)
        {
            if (Dispatcher.CheckAccess())
            {
                _ox.Value += ox;
                _oy.Value += oy;
            }
            else
            {
                Dispatcher.Invoke(() => IncrementXY(ox, oy));
            }
        }
    }
}