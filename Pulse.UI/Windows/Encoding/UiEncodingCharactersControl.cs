using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Pulse.UI.Encoding
{
    public sealed class UiEncodingCharactersControl : UiGrid
    {
        private readonly UiEncodingMainCharacterControl _mainControl;
        private readonly UiEncodingAdditionalCharacterControl _additionalControl;

        public UiEncodingWindowSource CurrentSource { get; private set; }
        public int CurrentIndex { get; private set; }
        public bool CurrentIsAdditional { get; private set; }

        public AutoResetEvent DrawEvent
        {
            set
            {
                _mainControl.DrawEvent = value;
                _additionalControl.DrawEvent = value;
            }
        }

        public UiEncodingCharactersControl()
        {
            #region Construct

            ColumnDefinitions.Add(new ColumnDefinition {Width = GridLength.Auto});

            _mainControl = new UiEncodingMainCharacterControl();
            {
                _mainControl.Visibility = Visibility.Hidden;
                AddUiElement(_mainControl, 0, 1);
            }

            _additionalControl = new UiEncodingAdditionalCharacterControl();
            {
                _additionalControl.Visibility = Visibility.Hidden;
                AddUiElement(_additionalControl, 0, 1);
            }

            #endregion
        }

        public void SetCurrent(UiEncodingWindowSource source, int index, bool isAdditional)
        {
            CurrentSource = source;
            CurrentIndex = index;
            CurrentIsAdditional = isAdditional;

            if (CurrentSource == null || CurrentIndex < 0)
            {
                _mainControl.Visibility = Visibility.Hidden;
                _additionalControl.Visibility = Visibility.Hidden;
                return;
            }

            if (CurrentIsAdditional)
            {
                _mainControl.Visibility = Visibility.Hidden;
                _additionalControl.Load(source, index);
                _additionalControl.Visibility = Visibility.Visible;
            }
            else
            {
                _additionalControl.Visibility = Visibility.Hidden;
                _mainControl.Load(source, index);
                _mainControl.Visibility = Visibility.Visible;
            }
        }

        public void IncrementXY(int ox, int oy)
        {
            if (CurrentSource == null || CurrentIndex < 0 || CurrentIsAdditional)
                return;

            _mainControl.IncrementXY(ox, oy);
        }
    }
}