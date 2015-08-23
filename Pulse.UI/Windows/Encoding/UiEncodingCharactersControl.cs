using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Pulse.Core;

namespace Pulse.UI.Encoding
{
    public sealed class UiEncodingCharactersControl : UiGrid
    {
        private readonly LazyArray<UiEncodingMainCharacterControl> _mainControls;
        private readonly LazyArray<UiEncodingAdditionalCharacterControl> _additionalControls;
        private readonly UiStackPanel _mainPanel;
        private readonly UiStackPanel _additionalPanel;

        public UiEncodingWindowSource CurrentSource { get; private set; }
        public IList<int> CurrentMainIndices;
        public IList<int> CurrentAdditionalIndices;
        private AutoResetEvent _drawEvent;

        public AutoResetEvent DrawEvent
        {
            set
            {
                _drawEvent = value;
                foreach (KeyValuePair<int, UiEncodingMainCharacterControl> control in _mainControls)
                    control.Value.DrawEvent = _drawEvent;
                foreach (KeyValuePair<int, UiEncodingAdditionalCharacterControl> control in _additionalControls)
                    control.Value.DrawEvent = _drawEvent;
            }
        }

        public UiEncodingCharactersControl()
        {
            #region Construct

            RowDefinitions.Add(new RowDefinition {Height = GridLength.Auto});
            RowDefinitions.Add(new RowDefinition {Height = GridLength.Auto});
            ColumnDefinitions.Add(new ColumnDefinition {Width = GridLength.Auto});

            _mainControls = new LazyArray<UiEncodingMainCharacterControl>(ProvideMainControl);
            _mainPanel = UiStackPanelFactory.Create(Orientation.Vertical);
            AddUiElement(_mainPanel, 0, 0);

            _additionalControls = new LazyArray<UiEncodingAdditionalCharacterControl>(ProvideAdditionalControl);
            _additionalPanel = UiStackPanelFactory.Create(Orientation.Vertical);
            AddUiElement(_additionalPanel, 1, 0);

            #endregion
        }

        private UiEncodingMainCharacterControl ProvideMainControl(int index)
        {
            if (!CheckAccess())
                return Dispatcher.Invoke(() => ProvideMainControl(index));

            UiEncodingMainCharacterControl control = new UiEncodingMainCharacterControl();
            {
                control.Visibility = Visibility.Collapsed;
                control.DrawEvent = _drawEvent;
                _mainPanel.AddUiElement(control);
                return control;
            }
        }

        private UiEncodingAdditionalCharacterControl ProvideAdditionalControl(int index)
        {
            if (!CheckAccess())
                return Dispatcher.Invoke(() => ProvideAdditionalControl(index));

            UiEncodingAdditionalCharacterControl control = new UiEncodingAdditionalCharacterControl();
            {
                control.Visibility = Visibility.Collapsed;
                control.DrawEvent = _drawEvent;
                _additionalPanel.AddUiElement(control);
                return control;
            }
        }

        public void SetCurrent(UiEncodingWindowSource source, IList<int> mainIndices, IList<int> additionalIndices)
        {
            CurrentSource = source;
            CurrentMainIndices = mainIndices;
            CurrentAdditionalIndices = additionalIndices;

            for (int i = mainIndices.Count; i < _mainControls.Count; i++)
                _mainControls[i].Visibility = Visibility.Collapsed;
            for (int i = additionalIndices.Count; i < _additionalControls.Count; i++)
                _additionalControls[i].Visibility = Visibility.Collapsed;

            if (CurrentSource == null)
                return;

            for (int i = 0; i < mainIndices.Count; i++)
            {
                _mainControls[i].Load(source, mainIndices[i]);
                _mainControls[i].Visibility = Visibility.Visible;
            }

            for (int i = 0; i < additionalIndices.Count; i++)
            {
                _additionalControls[i].Load(source, additionalIndices[i]);
                _additionalControls[i].Visibility = Visibility.Visible;
            }

            _drawEvent.NullSafeSet();
        }

        public void IncrementXY(int ox, int oy)
        {
            if (CurrentSource == null)
                return;

            for (int i = 0; i < CurrentMainIndices.Count; i++)
                _mainControls[i].IncrementXY(ox, oy);
        }
    }
}