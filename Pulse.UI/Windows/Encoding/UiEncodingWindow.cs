using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using Pulse.Core;
using Pulse.DirectX;
using Pulse.FS;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.Toolkit.Graphics;
using Rectangle = SharpDX.Rectangle;
using Color = System.Windows.Media.Color;
using SolidColorBrush = SharpDX.Direct2D1.SolidColorBrush;

namespace Pulse.UI.Encoding
{
    public sealed class UiEncodingWindow : UiWindow
    {
        private static readonly Color SelectedColor = Color.FromArgb(80, 255, 90, 90);
        private static readonly Color GridColor = Color.FromArgb(127, 127, 127, 127);
        private static readonly Color SpacingColor = Color.FromArgb(80, 90, 255, 90);
        private static readonly Color NotMappedColor = Color.FromArgb(80, 90, 90, 255);

        private readonly UiComboBox _comboBox;
        private readonly UiDxViewport _editViewport;
        private readonly UiDxViewport _previewViewport;
        private readonly UiEncodingCharactersControl _charactersControl;
        private UiEncodingWindowSource _currentSource;

        private readonly AutoResetEvent _moveEvent = new AutoResetEvent(false);
        private long _oldMovable, _newMovable;
        private int _oldX, _oldY, _deltaX, _deltaY;

        private float _scale = 1;
        private string _previewText = @"QUICKBROWNFOXJUMPSOVERTHELAZYDOG
quick brown fox jumps over the lazy dog
0123456789%/:!?…+-=*&「」()∙,.~#$_
СЪЕШЬЕЩЁЭТИХМЯГКИХФРАНЦУЗСКИХБУЛОКДАВЫПЕЙЧАЮ
съешь ещё этих мягких французских булок, да выпей чаю
АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯА
абвгдеёжзийклмнопрстуфхцчшщъыьэюяа";

        private SolidColorBrush _selectedColorBrush;
        private SolidColorBrush _spacingColorBrush;
        private SolidColorBrush _notMappedColorBrush;
        private SolidColorBrush _gridColorBrush;

        public UiEncodingWindow()
        {
            #region Construct

            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.CanMinimize;
            Width = 1024;
            Height = 768;

            UiGrid root = UiGridFactory.Create(4, 1);
            {
                root.RowDefinitions[0].Height = GridLength.Auto;
                root.RowDefinitions[2].Height = GridLength.Auto;
                root.RowDefinitions[3].Height = GridLength.Auto;

                _comboBox = UiComboBoxFactory.Create();
                {
                    _comboBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                    _comboBox.Margin = new Thickness(3);
                    _comboBox.DisplayMemberPath = "DisplayName";
                    _comboBox.SelectionChanged += OnComboBoxItemChanged;
                    root.AddUiElement(_comboBox, 0, 0);
                }

                _editViewport = new UiDxViewport();
                {
                    _editViewport.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                    _editViewport.VerticalAlignment = VerticalAlignment.Stretch;

                    _editViewport.DrawSprites += OnEditViewportDrawSprites;
                    _editViewport.DrawPrimitives += OnEditViewportDrawPrimitives;

                    _editViewport.DxControl.Control.MouseDown += OnDxControlElementMouseDown;
                    _editViewport.DxControl.Control.MouseUp += OnDxControlElementMouseUp;
                    _editViewport.DxControl.Control.MouseMove += OnDxControlElementMouseMove;

                    root.AddUiElement(_editViewport, 1, 0);
                }

                UiGrid previewGroup = UiGridFactory.Create(2, 2);
                {
                    previewGroup.RowDefinitions[0].Height = GridLength.Auto;
                    previewGroup.ColumnDefinitions[1].Width = GridLength.Auto;

                    _previewViewport = new UiDxViewport();
                    {
                        _previewViewport.Height = 200;
                        _previewViewport.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                        _previewViewport.VerticalAlignment = VerticalAlignment.Stretch;

                        _previewViewport.DrawSprites += OnPreviewViewportDraw;
                        _previewViewport.DxControl.RenderContainer.BackBuffer.BackgroundColor = Colors.Black;

                        previewGroup.AddUiElement(_previewViewport, 0, 0, 2);
                    }

                    UiEncodingLabeledNumber scale = new UiEncodingLabeledNumber("Масштаб:", 200, 100, 400, OnScaleValueChanged);
                    {
                        scale.Value = 100;
                        scale.NumberControl.Increment = 25;
                        previewGroup.AddUiElement(scale, 0, 1);
                    }

                    UiTextBox textBox = UiTextBoxFactory.Create();
                    {
                        textBox.Text = _previewText;
                        textBox.TextChanged += OnPreviewTextChanged;
                        previewGroup.AddUiElement(textBox, 1, 1);
                    }

                    root.AddUiElement(previewGroup, 2, 0);
                }

                _charactersControl = new UiEncodingCharactersControl();
                {
                    root.AddUiElement(_charactersControl, 3, 0);
                }

                UiButton button = UiButtonFactory.Create("OK");
                {
                    button.Width = 70;
                    button.Margin = new Thickness(3);
                    button.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    button.Click += (s, a) => DialogResult = true;
                    root.AddUiElement(button, 3, 0);
                }
            }
            Content = root;

            _editViewport.DxControl.RenderContainer.Reseted += ResetBurshes;
            ResetBurshes(_editViewport.DxControl.RenderContainer);

            Thread movingThread = new Thread(MovingThread);
            movingThread.Start();

            Activated += OnWindowActivated;
            Closing += (s, e) => ClosingEvent.Set();
            Closing += OnWindowClosing;

            #endregion
        }

        private readonly ManualResetEvent ClosingEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent ClosedEvent = new ManualResetEvent(true);

        private void ResetBurshes(RenderContainer renderContainer)
        {
            _selectedColorBrush?.Dispose();
            _spacingColorBrush?.Dispose();
            _notMappedColorBrush?.Dispose();
            _gridColorBrush?.Dispose();

            RenderTarget target2D = renderContainer.BackBuffer.Target2D;
            _selectedColorBrush = new SolidColorBrush(target2D, SelectedColor.ToColor4(), null);
            _spacingColorBrush = new SolidColorBrush(target2D, SpacingColor.ToColor4(), null);
            _notMappedColorBrush = new SolidColorBrush(target2D, NotMappedColor.ToColor4(), null);
            _gridColorBrush = new SolidColorBrush(target2D, GridColor.ToColor4(), null);
        }

        private void OnWindowActivated(object sender, EventArgs e)
        {
            if (_comboBox.SelectedIndex < 0 && _comboBox.Items.Count > 0)
                _comboBox.SelectedIndex = 0;
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            ClosedEvent.WaitOne();

            _selectedColorBrush.Dispose();
            _spacingColorBrush.Dispose();
            _notMappedColorBrush.Dispose();
            _gridColorBrush.Dispose();
        }

        private void OnScaleValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _scale = (int)e.NewValue / 100f;
            _previewViewport.Refresh();
        }

        private void OnPreviewTextChanged(object sender, TextChangedEventArgs e)
        {
            _previewText = ((UiTextBox)sender).Text;
            _previewViewport.Refresh();
        }

        public void Add(UiEncodingWindowSource source)
        {
            _comboBox.Items.Add(source);
        }

        private void OnDxControlElementMouseDown(object sender, MouseEventArgs e)
        {
            if (_currentSource == null)
                return;

            int viewportX = _editViewport.X;
            int viewportY = _editViewport.Y;

            List<int> mainIndices = new List<int>(2);
            List<int> additionalIndices = new List<int>(2);

            WflContent info = _currentSource.Info;
            int height = info.Header.LineHeight;
            for (int i = 0; i < 256 * 2; i++)
            {
                if (i % 256 < 0x20)
                    continue;

                int offsets = info.Offsets[i];
                int sizes = info.Sizes[i];
                int y = (offsets >> 16) & 0xFFFF;
                int oy = viewportY + e.Y - y;
                if (oy >= 0 && oy <= height)
                {
                    int x = offsets & 0xFFFF;
                    int width = (sizes & 0x0000FF00) >> 8;
                    int ox = viewportX + e.X - x;
                    if (ox >= 0 && ox <= width)
                    {
                        mainIndices.Add(i);
                        _newMovable = 1;
                    }
                }
            }

            int squareSize = info.Header.LineSpacing + info.Header.SquareDiff;
            short value = (short)(((viewportY + e.Y) / squareSize) << 8 | ((viewportX + e.X) / squareSize));
            int index = Array.IndexOf(info.AdditionalTable, value);
            if (index >= 0)
                additionalIndices.Add(index);

            _charactersControl.SetCurrent(_currentSource, mainIndices, additionalIndices);
            if (mainIndices.Count > 0 || additionalIndices.Count > 0)
                _editViewport.Refresh();
        }

        private void OnDxControlElementMouseUp(object sender, MouseEventArgs e)
        {
            _newMovable = 0;
        }

        private void OnDxControlElementMouseMove(object sender, MouseEventArgs e)
        {
            if (Interlocked.Exchange(ref _oldMovable, _newMovable) == 0)
            {
                _oldX = e.X;
                _oldY = e.Y;
            }
            else
            {
                int x = e.X;
                int y = e.Y;
                Interlocked.Add(ref _deltaX, x - _oldX);
                Interlocked.Add(ref _deltaY, y - _oldY);
                Interlocked.Exchange(ref _oldX, x);
                Interlocked.Exchange(ref _oldY, y);

                _moveEvent.Set();
            }
        }

        private void MovingThread()
        {
            while (ClosingEvent.WaitOne(200) == false)
            {
                if (_oldMovable == 0)
                    continue;

                int dx = Interlocked.Exchange(ref _deltaX, 0);
                int dy = Interlocked.Exchange(ref _deltaY, 0);

                _charactersControl.IncrementXY(dx, dy);
                _editViewport.Refresh();
            }
        }

        private void OnComboBoxItemChanged(object sender, EventArgs e)
        {
            _currentSource = (UiEncodingWindowSource)_comboBox.SelectedItem;
            _charactersControl.SetCurrent(_currentSource, new int[0], new int[0]);
            _editViewport.SetDesiredSize(_currentSource.Texture.Descriptor2D.Width, _currentSource.Texture.Descriptor2D.Height);
        }

        private void OnEditViewportDrawSprites(Device device, SpriteBatch spriteBatch, Rectangle cliprectangle)
        {
            UiEncodingWindowSource current = _currentSource;
            if (current == null)
                return;

            try
            {
                spriteBatch.Begin();
                current.Texture.Draw(device, spriteBatch, Vector2.Zero, new Rectangle(0, 0, current.Texture.Descriptor2D.Width, current.Texture.Descriptor2D.Height), 0, cliprectangle);
                spriteBatch.End();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private void OnEditViewportDrawPrimitives(Device device, RenderTarget target2D, Rectangle cliprectangle)
        {
            UiEncodingWindowSource source = _currentSource;

            WflContent info = source?.Info;
            if (info?.Header.TableType != WflHeader.LargeTable)
                return;

            int viewportX = _editViewport.X;
            int viewportY = _editViewport.Y;

            RectangleF rectangle = new RectangleF {Height = info.Header.LineHeight};

            target2D.BeginDraw();

            for (int i = 0; i < 256 * 2; i++)
            {
                if (i % 256 < 0x20)
                    continue;

                int x, y;
                info.GetOffsets(i, out x, out y);
                x -= viewportX;
                y -= viewportY;

                byte before, width, after;
                info.GetSizes(i, out before, out width, out after);

                rectangle.X = x;
                rectangle.Y = y;
                rectangle.Width = width & 0x7F;

                if (_charactersControl.CurrentMainIndices.Contains(i))
                {
                    target2D.FillRectangle(rectangle, _selectedColorBrush);

                    if (before > 0x7F)
                    {
                        rectangle.Width = (0xFF - before) + 1;
                        rectangle.X = x;
                    }
                    else
                    {
                        rectangle.Width = before;
                        rectangle.X = x - before;
                    }
                    target2D.FillRectangle(rectangle, _spacingColorBrush);

                    if (after > 0x7F)
                    {
                        rectangle.Width = (0xFF - after) + 1;
                        rectangle.X = x + (width & 0x7F) - rectangle.Width;
                    }
                    else
                    {
                        rectangle.Width = after;
                        rectangle.X = x + width & 0x7F;
                    }

                    target2D.FillRectangle(rectangle, _spacingColorBrush);
                }
                else if (source.Chars[i % 256] == 0x00)
                {
                    target2D.FillRectangle(rectangle, _notMappedColorBrush);
                }
                else
                {
                    target2D.DrawRectangle(rectangle, _gridColorBrush, 1.0f);
                }
            }

            int squareSize = info.Header.LineSpacing + info.Header.SquareDiff;
            rectangle.Height = squareSize;
            rectangle.Width = squareSize;
            for (int i = 0; i < info.AdditionalTable.Length; i++)
            {
                int value = info.AdditionalTable[i];
                if (value == 0)
                    continue;

                rectangle.Y = (value >> 8) * squareSize - viewportY;
                rectangle.X = (value & 0xFF) * squareSize - viewportX;

                if (_charactersControl.CurrentAdditionalIndices.Contains(i))
                    target2D.FillRectangle(rectangle, _selectedColorBrush);
                else if (source.Chars[i + 256] == 0x00)
                    target2D.FillRectangle(rectangle, _notMappedColorBrush);
                else
                    target2D.DrawRectangle(rectangle, _gridColorBrush, 1.0f);
            }

            target2D.EndDraw();
        }

        private void OnPreviewViewportDraw(Device device, SpriteBatch spriteBatch, Rectangle cliprectangle)
        {
            UiEncodingWindowSource source = _currentSource;
            if (source == null)
                return;

            spriteBatch.Begin(SpriteSortMode.Deferred, null, _previewViewport.DxControl.RenderContainer.GraphicsDevice.SamplerStates.PointClamp, null, null, null, SharpDX.Matrix.Scaling(_scale));

            float x = 0, maxX = 0;
            float y = source.Info.Header.LineSpacing;
            int squareSize = source.Info.Header.SquareSize;
            int lineSpacing = source.Info.Header.LineHeight + source.Info.Header.LineSpacing;

            foreach (char ch in _previewText)
            {
                if (ch == '\r')
                    continue;

                if (ch == '\n')
                {
                    x = 0;
                    y += lineSpacing;
                    continue;
                }

                short index;
                if (source.Codes.TryGetValue(ch, out index))
                {
                    int ox, oy;
                    int h, w;
                    if (index < 256)
                    {
                        byte before, width, after;
                        source.Info.GetSizes(index, out before, out width, out after);
                        w = width;
                        h = source.Info.Header.LineHeight;

                        if (before > 0x7F)
                            x = Math.Max(x - (0xFF - before), 0);
                        else
                            x += before;

                        source.Info.GetOffsets(index, out ox, out oy);
                        source.Texture.Draw(device, spriteBatch, new Vector2(x, y), new Rectangle(ox, oy, w, h), 0, cliprectangle);

                        if (after > 0x7F)
                            x = Math.Max(x - (0xFF - after), 0);
                        else
                            x += after;
                    }
                    else
                    {
                        index -= 256;
                        w = h = squareSize;
                        int value = source.Info.AdditionalTable[index];
                        ox = (value & 0xFF) * squareSize;
                        oy = (value >> 8) * squareSize;
                        source.Texture.Draw(device, spriteBatch, new Vector2(x, y), new Rectangle(ox, oy, w, h), 0, cliprectangle);
                    }

                    x += w;
                    maxX = Math.Max(x, maxX);
                }
            }

            spriteBatch.End();

            double desiredWidth, desiredHeight;
            _previewViewport.GetDesiredSize(out desiredWidth, out desiredHeight);

            double newDesiredWidth = x * _scale;
            double newDesiredHeight = (y + lineSpacing) * _scale;

            if (Math.Abs(newDesiredWidth - desiredWidth) > 1 || Math.Abs(newDesiredHeight - newDesiredHeight) > 1)
                _previewViewport.SetDesiredSize(newDesiredWidth, newDesiredHeight);
        }
    }
}