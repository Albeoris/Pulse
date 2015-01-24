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
using Pulse.UI.Controls;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using WindowState = System.Windows.WindowState;

namespace Pulse.UI.Encoding
{
    public sealed class UiEncodingWindow : UiWindow
    {
        private static readonly Color SelectedColor = Color.FromArgb(80, 255, 90, 90);
        private static readonly Color GridColor = Color.FromArgb(127, 127, 127, 127);
        private static readonly Color SpacingColor = Color.FromArgb(80, 90, 255, 90);
        private static readonly Color NotMappedColor = Color.FromArgb(80, 90, 90, 255);

        private readonly UiComboBox _comboBox;
        private readonly UiScrollViewer _viewer;
        private readonly UiGlControl _glControl;
        private readonly UiEncodingCharactersControl _charactersControl;
        private AutoResetEvent _drawEvent;
        private UiEncodingWindowSource _currentSource;

        private readonly AutoResetEvent _moveEvent = new AutoResetEvent(false);
        private long _oldMovable, _newMovable;
        private int _oldX, _oldY, _deltaX, _deltaY;
        
        public UiEncodingWindow()
        {
            #region Construct

            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.CanMinimize;
            Width = 1024;
            Height = 768;

            UiGrid root = UiGridFactory.Create(3, 1);
            {
                root.RowDefinitions[0].Height = GridLength.Auto;
                root.RowDefinitions[2].Height = GridLength.Auto;

                _comboBox = UiComboBoxFactory.Create();
                {
                    _comboBox.HorizontalAlignment = HorizontalAlignment.Stretch;
                    _comboBox.Margin = new Thickness(3);
                    _comboBox.DisplayMemberPath = "DisplayName";
                    _comboBox.SelectionChanged += OnComboBoxItemChanged;
                    root.AddUiElement(_comboBox, 0, 0);
                }

                _viewer = UiScrollViewerFactory.Create();
                {
                    _viewer.HorizontalAlignment = HorizontalAlignment.Left;
                    _viewer.VerticalAlignment = VerticalAlignment.Top;
                    _viewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                    _viewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;

                    _glControl = new UiScrollableGlControl();
                    {
                        _glControl.ClipToBounds = true;
                        _glControl.Control.Load += OnGLControlElementLoaded;
                        _glControl.Control.Resize += OnGLControlElementResize;
                        _glControl.Control.MouseDown += OnGLControlElementMouseDown;
                        _glControl.Control.MouseUp += OnGLControlElementMouseUp;
                        _glControl.Control.MouseMove += OnGLControlElementMouseMove;

                        _viewer.Content = _glControl;
                    }

                    root.AddUiElement(_viewer, 1, 0);
                }

                _charactersControl = new UiEncodingCharactersControl();
                {
                    root.AddUiElement(_charactersControl, 2, 0);
                }

                UiButton button = UiButtonFactory.Create("OK");
                {
                    button.Width = 70;
                    button.Margin = new Thickness(3);
                    button.HorizontalAlignment = HorizontalAlignment.Right;
                    button.Click += (s, a) => DialogResult = true;
                    root.AddUiElement(button, 2, 0);
                }
            }
            Content = root;

            Thread movingThread = new Thread(MovingThread);
            movingThread.Start();
            
            Closing += (s, e) => movingThread.Abort();

            #endregion
        }

        public void Add(UiEncodingWindowSource source)
        {
            //if (source.DisplayName == "wfnt18")
            //{
            //    Dictionary<char, char> dic = new Dictionary<char, char>();
            //    dic['А'] = 'A';     dic['A'] = 'А';
            //    dic['В'] = 'B';     dic['B'] = 'В';
            //    dic['Е'] = 'E';     dic['E'] = 'Е';
            //    dic['К'] = 'K';     dic['K'] = 'К';
            //    dic['М'] = 'M';     dic['M'] = 'М';
            //    dic['Н'] = 'H';     dic['H'] = 'Н';
            //    dic['О'] = 'O';     dic['O'] = 'О';
            //    dic['Р'] = 'P';     dic['P'] = 'Р';
            //    dic['С'] = 'C';     dic['C'] = 'С';
            //    dic['Т'] = 'T';     dic['T'] = 'Т';
            //    dic['Х'] = 'X';     dic['X'] = 'Х';
            //    dic['а'] = 'a';     dic['a'] = 'а';
            //    dic['е'] = 'e';     dic['e'] = 'е';
            //    dic['и'] = 'u';     dic['u'] = 'и';
            //    dic['о'] = 'o';     dic['o'] = 'о';
            //    dic['р'] = 'p';     dic['p'] = 'р';
            //    dic['с'] = 'c';     dic['c'] = 'с';
            //    dic['у'] = 'y';     dic['y'] = 'у';
            //    dic['х'] = 'x';     dic['x'] = 'х';

            //    for (int rusInd = 193; rusInd <= 255; rusInd++)
            //    {
            //        char eng;
            //        char rus = source.Chars[rusInd];
            //        if (dic.TryGetValue(rus, out eng))
            //        {
            //            short engCode = source.Codes[eng];
            //            source.Codes[rus] = engCode;
            //            source.Chars[rusInd] = '\0';
            //        }
            //    }

            //    int index = 193;
            //    for (int i = 66; i <= 90 && index < 256; i++)
            //    {
            //        char eng = source.Chars[i];
            //        if (dic.ContainsKey(eng))
            //            continue;

            //        while (index < 256)
            //        {
            //            char rus = source.Chars[index];
            //            if (rus == '\0' || dic.ContainsKey(rus))
            //            {
            //                index++;
            //                continue;
            //            }

            //            switch (rus)
            //            {
            //                case 'Ё':
            //                case 'ё':
            //                case 'Ы':
            //                case 'Й':
            //                case 'Ъ':
            //                case 'Ь':
            //                case 'Ц':
            //                case 'Ч':
            //                case 'Ш':
            //                case 'Щ':
            //                case 'Э':
            //                case 'ъ':
            //                    case 'Ж':
            //                    case 'Ю':
            //                    index++;
            //                    continue;
            //            }

            //            break;
            //        }

            //        if (index < 256)
            //        {
            //            Swap(source, i, index);
            //            index++;
            //        }
            //    }

            //    for (int i = 97; i <= 122 && index < 256; i++)
            //    {
            //        char eng = source.Chars[i];
            //        if (dic.ContainsKey(eng))
            //            continue;

            //        while (index < 256)
            //        {
            //            char rus = source.Chars[index];
            //            if (rus == '\0' || dic.ContainsKey(rus))
            //            {
            //                index++;
            //                continue;
            //            }

            //             switch (rus)
            //            {
            //                case 'Ё':
            //                case 'ё':
            //                case 'Ы':
            //                case 'Й':
            //                case 'Ъ':
            //                case 'Ь':
            //                case 'Ц':
            //                case 'Ч':
            //                case 'Ш':
            //                case 'Щ':
            //                case 'Э':
            //                case 'ъ':
            //                     case 'Ж':
            //                    case 'Ю':
            //                    index++;
            //                    continue;
            //            }

            //            break;
            //        }

            //        if (index < 256)
            //        {
            //            Swap(source, i, index);
            //            index++;
            //        }
            //    }
            //}
            _comboBox.Items.Add(source);
        }

        private void Swap(UiEncodingWindowSource source, int eng, int rus)
        {
            char engCh = source.Chars[eng];
            char rusCh = source.Chars[rus];

            short engCode = source.Codes[engCh];
            short rusCode = source.Codes[rusCh];
            source.Codes[engCh] = rusCode;
            source.Codes[rusCh] = engCode;

            source.Chars.Swap(eng, rus);
            source.Info.Offsets.Swap(eng, rus);
            source.Info.Sizes.Swap(eng, rus);
            source.Info.Offsets.Swap(eng+256, rus+256);
            source.Info.Sizes.Swap(eng+256, rus+256);
        }

        private void OnGLControlElementLoaded(object sender, EventArgs e)
        {
            Activated += OnWindowActivated;
            Deactivated += OnWindowDeactivated;
            StateChanged += OnWindowStateChanged;
            ContentRendered += OnWindowContentRendered;
        }

        private void OnGLControlElementResize(object sender, EventArgs e)
        {
            ConfigPreview();
        }

        private void OnGLControlElementMouseDown(object sender, MouseEventArgs e)
        {
            if (_currentSource == null)
                return;

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
                        _charactersControl.SetCurrent(_currentSource, i, false);
                        _drawEvent.Set();
                        _newMovable = 1;
                        return;
                    }
                }
            }

            int squareSize = info.Header.LineSpacing + info.Header.SquareDiff;
            short value = (short)((e.Y / squareSize) << 8 | (e.X / squareSize));
            int index = Array.IndexOf(info.AdditionalTable, value);
            _charactersControl.SetCurrent(_currentSource, index, true);
            _drawEvent.Set();
            _newMovable = 1;
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
            GLService.SubscribeControl(_glControl);
            _drawEvent = GLService.RegisterDrawMethod(Draw);
            _charactersControl.DrawEvent = _drawEvent;
            ConfigPreview();
        }

        private void OnWindowDeactivated(object sender, EventArgs e)
        {
            GLService.UnregisterDrawMethod(Draw);
            GLService.UnsubscribeControl(_glControl);
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
            _charactersControl.SetCurrent(_currentSource, -1, false);
            int height = _currentSource.Texture.Height + (_currentSource.Info.Header.LineHeight + _currentSource.Info.Header.LineSpacing) * 4;
            GLService.SetViewportDesiredSize(_currentSource.Texture.Width, height);
        }

        private void ConfigPreview()
        {
            using (_glControl.AcquireContext())
            {
                GL.ClearColor(Color4.Black);

                int w = Math.Max(1, _glControl.Control.Width);
                int h = Math.Max(1, _glControl.Control.Height);
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                GL.Ortho(0, w, h, 0, -1, 1);
                GL.Viewport(0, 0, w, h);

                _drawEvent.NullSafeSet();
            }
        }

        private void Draw()
        {
            UiEncodingWindowSource current = _currentSource;
            if (current == null)
                return;

            using (_glControl.AcquireContext())
            {
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadIdentity();

                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                GL.Color3(Color.Transparent);

                current.Texture.Draw(0, 0, 0);

                const string PreviwText = "QUICKBROWNFOXJUMPSOVERTHELAZYDOG\r\nquick brown fox jumps over the lazy dog\r\n0123456789%/:!?…+-=*&「」()∙,.~#$_\r\nСЪЕШЬЕЩЁЭТИХМЯГКИХФРАНЦУЗСКИХБУЛОК\r\nсъешь ещё этих мягких французских булок";

                DrawCharacters(current);
                DrawPreview(current, PreviwText);

                GL.Disable(EnableCap.Blend);

                _glControl.SwapBuffers();
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

                if (!_charactersControl.CurrentIsAdditional && i == _charactersControl.CurrentIndex)
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

                if (_charactersControl.CurrentIsAdditional && _charactersControl.CurrentIndex == i)
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
            float y = source.Texture.Height + source.Info.Header.LineSpacing;
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