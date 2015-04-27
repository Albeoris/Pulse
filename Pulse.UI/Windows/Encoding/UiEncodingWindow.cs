using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Pulse.Core;
using Pulse.FS;
using Pulse.OpenGL;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

namespace Pulse.UI.Encoding
{
    public sealed class UiEncodingWindow : UiWindow
    {
        private static readonly Color SelectedColor = Color.FromArgb(80, 255, 90, 90);
        private static readonly Color GridColor = Color.FromArgb(127, 127, 127, 127);
        private static readonly Color SpacingColor = Color.FromArgb(80, 90, 255, 90);
        private static readonly Color NotMappedColor = Color.FromArgb(80, 90, 90, 255);

        private readonly UiComboBox _comboBox;
        private readonly UiScrollViewer _glControlViewer;
        private readonly UiScrollViewer _glPreviewViewer;
        private readonly UiGlControl _glEditControl;
        private readonly UiGlControl _glPreviewControl;
        private readonly UiEncodingCharactersControl _charactersControl;
        private AutoResetEvent _drawEvent;
        private UiEncodingWindowSource _currentSource;

        private readonly AutoResetEvent _moveEvent = new AutoResetEvent(false);
        private long _oldMovable, _newMovable;
        private int _oldX, _oldY, _deltaX, _deltaY;
        private int _glControlLoadingCounter;
        
        private float _scale = 1;
        private string _previewText = @"QUICKBROWNFOXJUMPSOVERTHELAZYDOG
quick brown fox jumps over the lazy dog
0123456789%/:!?…+-=*&「」()∙,.~#$_
СЪЕШЬЕЩЁЭТИХМЯГКИХФРАНЦУЗСКИХБУЛОКДАВЫПЕЙЧАЮ
съешь ещё этих мягких французских булок, да выпей чаю
АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯА
абвгдеёжзийклмнопрстуфхцчшщъыьэюяа";

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
                    _comboBox.HorizontalAlignment = HorizontalAlignment.Stretch;
                    _comboBox.Margin = new Thickness(3);
                    _comboBox.DisplayMemberPath = "DisplayName";
                    _comboBox.SelectionChanged += OnComboBoxItemChanged;
                    root.AddUiElement(_comboBox, 0, 0);
                }

                _glControlViewer = UiScrollViewerFactory.Create();
                {
                    _glControlViewer.HorizontalAlignment = HorizontalAlignment.Left;
                    _glControlViewer.VerticalAlignment = VerticalAlignment.Top;
                    _glControlViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                    _glControlViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;

                    _glEditControl = new UiScrollableGlControl();
                    {
                        _glEditControl.ClipToBounds = true;
                        _glEditControl.Control.Load += OnGLControlElementLoaded;
                        _glEditControl.Control.Resize += OnGLControlElementResize;
                        _glEditControl.Control.MouseDown += OnGLControlElementMouseDown;
                        _glEditControl.Control.MouseUp += OnGLControlElementMouseUp;
                        _glEditControl.Control.MouseMove += OnGLControlElementMouseMove;

                        _glControlViewer.Content = _glEditControl;
                    }

                    root.AddUiElement(_glControlViewer, 1, 0);
                }

                UiGrid previewGroup = UiGridFactory.Create(2, 2);
                {
                    previewGroup.RowDefinitions[0].Height = GridLength.Auto;
                    previewGroup.ColumnDefinitions[1].Width = GridLength.Auto;
                
                    _glPreviewViewer = UiScrollViewerFactory.Create();
                    {
                        _glPreviewViewer.Height = 200;
                        _glPreviewViewer.HorizontalAlignment = HorizontalAlignment.Left;
                        _glPreviewViewer.VerticalAlignment = VerticalAlignment.Top;
                        _glPreviewViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                        _glPreviewViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;

                        _glPreviewControl = new UiScrollableGlControl();
                        {
                            _glPreviewControl.ClipToBounds = true;
                            _glPreviewControl.Control.Load += OnGLControlElementLoaded;
                            _glPreviewControl.Control.Resize += OnGLControlElementResize;

                            _glPreviewViewer.Content = _glPreviewControl;
                        }

                        previewGroup.AddUiElement(_glPreviewViewer, 0, 0, 2);

                        UiEncodingLabeledNumber scale = new UiEncodingLabeledNumber("Масштаб:", 200, 100, 400, OnScaleValueChanged);
                        {
                            scale.Value = 100;
                            previewGroup.AddUiElement(scale, 0, 1);
                        }

                        UiTextBox textBox = UiTextBoxFactory.Create();
                        {
                            textBox.Text = _previewText;
                            textBox.TextChanged += OnPreviewTextChanged;
                            previewGroup.AddUiElement(textBox, 1, 1);
                        }
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
                    button.HorizontalAlignment = HorizontalAlignment.Right;
                    button.Click += (s, a) => DialogResult = true;
                    root.AddUiElement(button, 3, 0);
                }
            }
            Content = root;

            Thread movingThread = new Thread(MovingThread);
            movingThread.Start();
            
            Closing += (s, e) => movingThread.Abort();

            #endregion
        }

        private void OnScaleValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _scale =  (int)e.NewValue / 100f;
            _drawEvent.NullSafeSet();
        }

        private void OnPreviewTextChanged(object sender, TextChangedEventArgs e)
        {
            _previewText = ((UiTextBox)sender).Text;
            _drawEvent.Set();
        }

        public void Add(UiEncodingWindowSource source)
        {
            _comboBox.Items.Add(source);
        }

        private void OnGLControlElementLoaded(object sender, EventArgs e)
        {
            if (Interlocked.Increment(ref _glControlLoadingCounter) % 2 != 0)
                return;

            Activated += OnWindowActivated;
            Deactivated += OnWindowDeactivated;
            StateChanged += OnWindowStateChanged;
            ContentRendered += OnWindowContentRendered;
        }

        private void OnGLControlElementResize(object sender, EventArgs e)
        {
            ConfigGlEdit();
            ConfigGlPrview();
        }

        private void OnGLControlElementMouseDown(object sender, MouseEventArgs e)
        {
            if (_currentSource == null)
                return;

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
                int oy = e.Y - y;
                if (oy >= 0 && oy <= height)
                {
                    int x = offsets & 0xFFFF;
                    int width = (sizes & 0x0000FF00) >> 8;
                    int ox = e.X - x;
                    if (ox >= 0 && ox <= width)
                    {
                        mainIndices.Add(i);
                        _newMovable = 1;
                    }
                }
            }

            int squareSize = info.Header.LineSpacing + info.Header.SquareDiff;
            short value = (short)((e.Y / squareSize) << 8 | (e.X / squareSize));
            int index = Array.IndexOf(info.AdditionalTable, value);
            if (index >= 0)
                additionalIndices.Add(index);

            _charactersControl.SetCurrent(_currentSource, mainIndices, additionalIndices);
            if (mainIndices.Count > 0 || additionalIndices.Count > 0)
                _drawEvent.Set();
        }

        private void OnGLControlElementMouseUp(object sender, MouseEventArgs e)
        {
            _newMovable = 0;
        }

        private void OnGLControlElementMouseMove(object sender, MouseEventArgs e)
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
            while (true)
            {
                if (_oldMovable == 0)
                {
                    Thread.Sleep(200);
                    continue;
                }

                int dx = Interlocked.Exchange(ref _deltaX, 0);
                int dy = Interlocked.Exchange(ref _deltaY, 0);
                
                _charactersControl.IncrementXY(dx, dy);
            }
        }

        private void OnWindowActivated(object sender, EventArgs e)
        {
            GLService.SubscribeControl(_glEditControl);
            GLService.SubscribeControl(_glPreviewControl);
            _charactersControl.DrawEvent = _drawEvent = GLService.RegisterDrawMethod(DrawEdit);

            if (_comboBox.SelectedIndex < 0 && _comboBox.Items.Count > 0)
                _comboBox.SelectedIndex = 0;
        }

        private void OnWindowDeactivated(object sender, EventArgs e)
        {
            GLService.UnregisterDrawMethod(DrawEdit);
            GLService.UnsubscribeControl(_glEditControl);
            GLService.UnsubscribeControl(_glPreviewControl);
        }

        private void OnWindowStateChanged(object sender, EventArgs e)
        {
            if (WindowState != WindowState.Minimized)
                _drawEvent.NullSafeSet();
        }

        private void OnWindowContentRendered(object sender, EventArgs e)
        {
            _drawEvent.NullSafeSet();
        }

        private void OnComboBoxItemChanged(object sender, EventArgs e)
        {
            _currentSource = (UiEncodingWindowSource)_comboBox.SelectedItem;
            _charactersControl.SetCurrent(_currentSource, new int[0], new int[0]);
            //_glEditControl.SetViewportDesiredSize(_currentSource.Texture.Width, _currentSource.Texture.Height);
            //_glPreviewControl.SetViewportDesiredSize(_currentSource.Texture.Width * 4, (_currentSource.Info.Header.LineHeight + _currentSource.Info.Header.LineSpacing) * 8 * 4);
            GLService.SetViewportDesiredSize(_currentSource.Texture.Width, _currentSource.Texture.Height);
        }

        private void ConfigGlEdit()
        {
            if (!_glEditControl.IsLoaded)
                return;

            using (_glEditControl.AcquireContext())
            {
                GL.ClearColor(Color4.Black);

                int w = Math.Max(1, _glEditControl.Control.Width);
                int h = Math.Max(1, _glEditControl.Control.Height);
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                GL.Ortho(0, w, h, 0, -1, 1);
                GL.Viewport(0, 0, w, h);

                _drawEvent.NullSafeSet();
            }
        }

        private void ConfigGlPrview()
        {
            if (!_glPreviewControl.IsLoaded)
                return;

            using (_glPreviewControl.AcquireContext())
            {
                GL.ClearColor(Color4.Black);

                int w = Math.Max(1, _glPreviewControl.Control.Width);
                int h = Math.Max(1, _glPreviewControl.Control.Height);
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                GL.Ortho(0, w, h, 0, -1, 1);
                GL.Viewport(0, 0, w, h);

                _drawEvent.NullSafeSet();
            }
        }

        private void DrawEdit()
        {
            UiEncodingWindowSource current = _currentSource;
            if (current == null)
                return;

            try
            {
                using (_glEditControl.AcquireContext())
                {
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                    GL.MatrixMode(MatrixMode.Modelview);
                    GL.LoadIdentity();
                    GL.Scale(1, 1, 1);

                    current.Texture.Draw(0, 0, 0);

                    GL.Enable(EnableCap.Blend);
                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                    GL.Color3(Color.Transparent);

                    DrawCharacters(current);

                    GL.Disable(EnableCap.Blend);

                    _glEditControl.SwapBuffers();
                }

                using (_glPreviewControl.AcquireContext())
                {
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                    GL.MatrixMode(MatrixMode.Modelview);
                    GL.LoadIdentity();
                    GL.Scale(_scale, _scale, _scale);

                    //GL.Enable(EnableCap.Blend);
                    //GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                    //GL.Color3(Color.Transparent);

                    DrawPreview(current, _previewText);

                    //GL.Disable(EnableCap.Blend);

                    _glPreviewControl.SwapBuffers();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private void DrawCharacters(UiEncodingWindowSource source)
        {
            WflContent info = source.Info;
            if (info.Header.TableType != WflHeader.LargeTable)
                return;

            GLRectangle rectangle = new GLRectangle {Height = info.Header.LineHeight};

            for (int i = 0; i < 256 * 2; i++)
            {
                if (i % 256 < 0x20)
                    continue;

                int x, y;
                info.GetOffsets(i, out x, out y);

                byte before, width, after;
                info.GetSizes(i, out before, out width, out after);

                rectangle.X = x;
                rectangle.Y = y;
                rectangle.Width = width & 0x7F;

                if (_charactersControl.CurrentMainIndices.Contains(i))
                {
                    rectangle.DrawSolid(SelectedColor);

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
                    rectangle.DrawSolid(SpacingColor);

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
                    rectangle.DrawSolid(SpacingColor);
                }
                else if (source.Chars[i % 256] == 0x00)
                {
                    rectangle.DrawSolid(NotMappedColor);
                }
                else
                {
                    rectangle.DrawBorder(GridColor);
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

                rectangle.Y = (value >> 8) * squareSize;
                rectangle.X = (value & 0xFF) * squareSize;

                if (_charactersControl.CurrentAdditionalIndices.Contains(i))
                    rectangle.DrawSolid(SelectedColor);
                else if (source.Chars[i + 256] == 0x00)
                    rectangle.DrawSolid(NotMappedColor);
                else
                    rectangle.DrawBorder(GridColor);
            }
        }
private void DrawPreview(UiEncodingWindowSource source, string text)
        {
            GL.Color3(Color.Transparent);

            float x = 0;
            float y = source.Info.Header.LineSpacing; // + source.Texture.Height
            int squareSize = source.Info.Header.SquareSize;
            int lineSpacing = source.Info.Header.LineHeight + source.Info.Header.LineSpacing;

            foreach (char ch in text)
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
                    float h, w;
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
                        source.Texture.Draw(x, y, 0, ox, oy, w, h);

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
                        source.Texture.Draw(x, y, 0, ox, oy, w, h);
                    }

                    x += w;
                }
            }
        }
    }
}