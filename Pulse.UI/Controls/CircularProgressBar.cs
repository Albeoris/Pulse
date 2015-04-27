using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Pulse.UI
{
    public sealed class UiCircularProgressIcon : UserControl
    {
        private static readonly Brush StrokeBrush;
        private const double StrokeThickness = 3;

        static UiCircularProgressIcon()
        {
            StrokeBrush = new SolidColorBrush(Color.FromArgb(90, 00, 00, 00));
            StrokeBrush.Freeze();
        }

        private readonly double _radius;
        private readonly Polyline _polyLine;
        private readonly DispatcherTimer _timer;

        private double _value, _maximum, _oldAngle, _newAngle;

        public UiCircularProgressIcon()
            : this(16)
        {

        }

        public UiCircularProgressIcon(double diameter)
        {
            Width = diameter;
            Height = diameter;

            _radius = (diameter - StrokeThickness) / 2;

            _polyLine = new Polyline();
            _polyLine.Loaded += (s, e) => _timer.Start();
            _polyLine.Dispatcher.ShutdownStarted += (s, e) => _timer.Stop();
            _polyLine.StrokeThickness = StrokeThickness;
            _polyLine.Stroke = StrokeBrush;

            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(15), DispatcherPriority.Render, Draw, _polyLine.Dispatcher);
            _timer.Start();
            VerticalContentAlignment = VerticalAlignment.Center;
            HorizontalContentAlignment = HorizontalAlignment.Center;
            Content = _polyLine;
        }

        public double Maximum
        {
            set { Update(_value, value); }
        }

        public double Value
        {
            set { Update(value, _maximum); }
        }

        public void SetMaximum(long value)
        {
            Maximum = value;
        }

        public void IncrementValue(long delta)
        {
            Value = _value + delta;
        }

        private void Update(double value, double maximum)
        {
            _maximum = maximum;
            _value = Math.Min(value, _maximum);

            _newAngle = _value / _maximum * Math.PI * 2;

            if (_newAngle < _oldAngle)
            {
                _timer.Stop();
                if (_polyLine.CheckAccess())
                    _polyLine.Points.Clear();
                else
                    _polyLine.Dispatcher.Invoke(()=>_polyLine.Points.Clear());

                _oldAngle = 0;
            }
            else if (!_timer.IsEnabled)
            {
                _timer.Start();
            }
        }

        private void Draw(object sender, EventArgs e)
        {
            const double step = 3.6 * Math.PI / 180;

            while (_oldAngle < _newAngle)
            {
                _oldAngle += step;
                double x = _radius + _radius * Math.Cos(_oldAngle - Math.PI / 2);
                double y = _radius + _radius * Math.Sin(_oldAngle - Math.PI / 2);
                _polyLine.Points.Add(new Point(x, y));
            }
        }
    }
}