using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Pulse.Core;
using Pulse.DirectX;
using SharpDX;

namespace Pulse.UI
{
    public class UiDxViewport : UserControl
    {
        private readonly UiGrid _grid;
        private readonly ScrollBar _verticalScrollBar;
        private readonly ScrollBar _horizontalScrollBar;

        public readonly UiDxControl  DxControl;
        private double _horizontalOffset;
        private double _verticalOffset;
        private double _desiredWidth;
        private double _desiredHeight;
        private double _actualWidth;
        private double _actualHeight;

        public int X => (int)_horizontalOffset;
        public int Y => (int)_verticalOffset;

        public UiDxViewport()
        {
            _grid = UiGridFactory.Create(2, 2);
            _grid.ColumnDefinitions[1].Width = GridLength.Auto;
            _grid.RowDefinitions[1].Height = GridLength.Auto;

            _verticalScrollBar = new ScrollBar {Orientation = Orientation.Vertical};
            {
                _verticalScrollBar.ValueChanged += OnVerticalScroll;
                _grid.AddUiElement(_verticalScrollBar, 0, 1);
            }

            _horizontalScrollBar = new ScrollBar {Orientation = Orientation.Horizontal};
            {
                _horizontalScrollBar.ValueChanged += OnHorizontalScroll;
                _grid.AddUiElement(_horizontalScrollBar, 1, 0);
            }

            DxControl = new UiDxControl();
            {
                DxControl.SizeChanged += OnDxControlSizeChanged;
                _grid.AddUiElement(DxControl, 0, 0);
            }

            VerticalAlignment = VerticalAlignment.Stretch;
            HorizontalAlignment = HorizontalAlignment.Stretch;

            Content = _grid;
        }

        public event UiDxControl.DrawSpritesDelegate DrawSprites
        {
            add { DxControl.DrawSprites += (device, spriteBatch, clipRectangle) => value(device, spriteBatch, new Rectangle(X, Y, clipRectangle.Width, clipRectangle.Height)); }
            remove { throw new NotSupportedException(); }
        }

        public event UiDxControl.DrawPrimitivesDelegate DrawPrimitives
        {
            add { DxControl.DrawPrimitives += (device, primitivesBatch, clipRectangle) => value(device, primitivesBatch, new Rectangle(X, Y, clipRectangle.Width, clipRectangle.Height)); }
            remove { throw new NotSupportedException(); }
        }

        public void GetDesiredSize(out double width, out double height)
        {
            width = _desiredWidth;
            height = _desiredHeight;
        }

        public void SetDesiredSize(double width, double height)
        {
            _desiredWidth = width;
            _desiredHeight = height;
            UpdateScrollBars();
        }

        public void GetActualSize(out double width, out double height)
        {
            width = _actualWidth;
            height = _actualHeight;
        }

        public void SetActualSize(double width, double height)
        {
            _actualWidth = width;
            _actualHeight = height;
            UpdateScrollBars();
        }

        public void Refresh()
        {
            DxControl.Control.BeginInvoke(new Action(()=>DxControl.Control.Refresh()));
        }

        private void UpdateScrollBars()
        {
            double horizontalMaximum = Math.Max(0, _desiredWidth - _actualWidth);
            double verticalMaximum = Math.Max(0, _desiredHeight - _actualHeight);

            _horizontalOffset = Math.Min(_horizontalScrollBar.Value, horizontalMaximum);
            _verticalOffset = Math.Min(_verticalScrollBar.Value, verticalMaximum);

            _horizontalScrollBar.ViewportSize = _actualWidth;
            _verticalScrollBar.ViewportSize = _actualHeight;
            _horizontalScrollBar.Maximum = horizontalMaximum;
            _verticalScrollBar.Maximum = verticalMaximum;
            _horizontalScrollBar.Value = _horizontalOffset;
            _verticalScrollBar.Value = _verticalOffset;

            Refresh();
        }

        private void OnDxControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                SetActualSize(e.NewSize.Width, e.NewSize.Height);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private void OnHorizontalScroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                _horizontalOffset = (float)e.NewValue;
                e.Handled = true;

                Refresh();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private void OnVerticalScroll(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                _verticalOffset = (float)e.NewValue;
                e.Handled = true;

                Refresh();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}