using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.WindowsAPICodePack.Dialogs;
using Pulse.Core;
using Pulse.FS;
using Pulse.OpenGL;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

// ReSharper disable UnusedMember.Local

namespace Pulse.UI
{
    public sealed class UiGameFilePreviewYkd : UiGrid
    {
        private readonly Tree _treeView;
        private readonly PropertyGrid _propertyGrid;
        private readonly UiGlViewport _viewer;
        private UiButton _rollbackButton;
        private UiButton _injectButton;
        private UiButton _saveAsButton;

        public UiGameFilePreviewYkd()
        {
            #region Constructor

            SetCols(2);
            SetRows(3);
            RowDefinitions[1].Height = new GridLength();
            RowDefinitions[2].Height = new GridLength();

            _treeView = new Tree();
            _treeView.SelectedItemChanged += OnTreeViewSelectedItemChanged;
            AddUiElement(_treeView, 0, 0, 2);

            _propertyGrid = new PropertyGrid {AutoGenerateProperties = true};
            AddUiElement(_propertyGrid, 0, 1);

            _viewer = new UiGlViewport(Draw);
            AddUiElement(_viewer, 1, 1);

            _rollbackButton = UiButtonFactory.Create("Отменить");
            {
                _rollbackButton.Width = 200;
                _rollbackButton.Margin = new Thickness(5);
                _rollbackButton.HorizontalAlignment = HorizontalAlignment.Left;
                _rollbackButton.VerticalAlignment = VerticalAlignment.Center;
                _rollbackButton.Click += OnRolbackButtonClick;
                AddUiElement(_rollbackButton, 2, 0, 0, 2);
            }

            _injectButton = UiButtonFactory.Create("Вставить");
            {
                _injectButton.Width = 200;
                _injectButton.Margin = new Thickness(5, 5, 210, 5);
                _injectButton.HorizontalAlignment = HorizontalAlignment.Right;
                _injectButton.VerticalAlignment = VerticalAlignment.Center;
                _injectButton.Click += OnInjectButtonClick;
                AddUiElement(_injectButton, 2, 0, 0, 2);
            }

            _saveAsButton = UiButtonFactory.Create("Сохранить как...");
            {
                _saveAsButton.Width = 200;
                _saveAsButton.Margin = new Thickness(5);
                _saveAsButton.HorizontalAlignment = HorizontalAlignment.Right;
                _saveAsButton.VerticalAlignment = VerticalAlignment.Center;
                _saveAsButton.Click += OnSaveAsButtonClick;
                AddUiElement(_saveAsButton, 2, 0, 0, 2);
            }

            #endregion
        }

        private WpdArchiveListing _listing;
        private WpdEntry _entry;
        private YkdFile _ykdFile;
        private Dictionary<string, GLTexture> _textures;

        public void Show(WpdArchiveListing listing, WpdEntry entry)
        {
            _listing = listing;
            _entry = entry;

            if (_textures != null)
            {
                foreach (GLTexture texture in _textures.Values)
                    texture.Dispose();

                _textures = null;
            }

            if (listing == null || entry == null)
            {
                _treeView.ItemsSource = null;
                return;
            }

            using (Stream headers = listing.Accessor.ExtractHeaders())
            {
                _ykdFile = new StreamSegment(headers, entry.Offset, entry.Length, FileAccess.Read).ReadContent<YkdFile>();
                _textures = _ykdFile.Resources.Resources
                    .Select(r => _listing.FirstOrDefault(e => e.NameWithoutExtension == r.Name))
                    .Distinct()
                    .Where(e => e != null)
                    .ToDictionary(wpdEntry => wpdEntry.NameWithoutExtension, e => GLTextureReader.ReadFromWpd(listing, e));

                _treeView.ItemsSource = new[] {new YkdFileView(_ykdFile)};
            }

            Visibility = Visibility.Visible;
            _viewer.DrawEvent.Set();
        }

        private int _w, _h;

        private void Draw()
        {
            YkdFile file = _ykdFile;
            if (file == null)
                return;

            int w = 0, h = 0;
            using (_viewer.AcquireContext())
            {
                foreach (YkdResource resource in file.Resources.Resources)
                {
                    GLTexture texture = _textures.TryGetValue(resource.Name);
                    if (texture == null)
                        continue;

                    DrawTexture(ref w, ref h, resource);
                }
            }

            if (_w != w || _h != h)
            {
                _w = w;
                _h = h;
                GLService.SetViewportDesiredSize(_w, _h);
            }

            _viewer.SwapBuffers();
        }

        private void DrawTexture(ref int w, ref int h, YkdResource resource)
        {
            GLTexture texture = _textures.TryGetValue(resource.Name);
            if (texture == null)
                return;

            switch (resource.Viewport.Type)
            {
                case YkdResourceViewportType.Fragment:
                    FragmentYkdResourceViewport fragment = (FragmentYkdResourceViewport)resource.Viewport;
                    texture.Draw(w, 0, 0, fragment.SourceX, fragment.SourceY, fragment.SourceWidth, fragment.SourceHeight);
                    w += fragment.SourceWidth;
                    h = Math.Max(h, fragment.SourceHeight);
                    break;
                case YkdResourceViewportType.Full:
                    FullYkdResourceViewport full = (FullYkdResourceViewport)resource.Viewport;
                    texture.Draw(w, 0, 0, 0, 0, full.ViewportWidth, full.ViewportHeight);
                    w += full.ViewportWidth;
                    h = Math.Max(h, full.ViewportHeight);
                    break;
                case YkdResourceViewportType.Extra:
                    ExtraYkdResourceViewport extra = (ExtraYkdResourceViewport)resource.Viewport;
                    texture.Draw(w, 0, 0, 0, 0, extra.SourceWidth, extra.SourceHeight);
                    w += extra.SourceWidth;
                    h = Math.Max(h, extra.SourceHeight);
                    break;
            }
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _propertyGrid.SelectedObject = _treeView.SelectedItem as View;
        }

        private void OnRolbackButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                IsEnabled = false;
                Show(_listing, _entry);
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(this, ex);
            }
            finally
            {
                IsEnabled = true;
            }
        }

        private void OnInjectButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_ykdFile == null)
                    return;

                MemoryStream output = new MemoryStream(32 * 1024);
                _ykdFile.WriteToStream(output);

                UiWpdInjector.InjectSingle(_listing, _entry, output);
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(this, ex);
            }
        }

        private void OnSaveAsButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_ykdFile == null)
                    return;

                String targetPath;
                using (CommonSaveFileDialog dlg = new CommonSaveFileDialog("Сохранить как..."))
                {
                    dlg.DefaultFileName = _entry.Name;
                    if (dlg.ShowDialog() != CommonFileDialogResult.Ok)
                        return;

                    targetPath = dlg.FileName;
                }

                using (FileStream output = File.Create(targetPath))
                    _ykdFile.WriteToStream(output);
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(this, ex);
            }
        }


        // =======================================================================

        private sealed class Tree : UiTreeView
        {
            public Tree()
            {
                ItemTemplateSelector = new TemplateSelector();
            }

            private sealed class TemplateSelector : DataTemplateSelector
            {
                public override DataTemplate SelectTemplate(object item, DependencyObject container)
                {
                    View view = item as View;
                    return view == null ? null : view.TreeViewTemplate;
                }
            }
        }

        private abstract class View : INotifyPropertyChanged
        {
            [Browsable(false)]
            public abstract DataTemplate TreeViewTemplate { get; }

            // Binding
            [Browsable(false)]
            public IEnumerable<View> BindableChilds
            {
                get { return EnumerateChilds(); }
            }

            protected abstract IEnumerable<View> EnumerateChilds();


            protected delegate Boolean StringToValueConverter<T>(String value, out T result);

            protected String ReadArrayAsString<T>(T[] array, Func<T, String> valueToString)
            {
                if (array == null)
                    return null;

                return String.Join(", ", array.Select(valueToString));
            }

            protected void WriteArrayFromString<T>(T[] array, String value, StringToValueConverter<T> stringToValue)
            {
                if (array == null)
                    return;

                int index = 0;
                foreach (string val in (value ?? string.Empty).Split(','))
                {
                    if (index >= array.Length)
                        break;

                    T number;
                    if (stringToValue(val, out number))
                        array[index] = number;

                    index++;
                }
            }

            protected string ReadInt32ArrayAsString(int[] array)
            {
                return ReadArrayAsString(array, v => v.ToString("X8"));
            }

            protected void WriteInt32ArrayFromString(int[] array, String value)
            {
                WriteArrayFromString(array, value, TryParseInt32);
            }

            private static bool TryParseInt32(string value, out int result)
            {
                return int.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result);
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected internal void RaisePropertyChanged(string propertyName)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                    handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private abstract class View<TView, TNative> : View
        {
            protected internal readonly TNative Native;

            protected View(TNative native)
            {
                Native = native;
            }

            // Binding
            // ReSharper disable once MemberCanBeProtected.Global
            public virtual String Title
            {
                get { return TypeCache<TNative>.Type.Name; }
            }

            public virtual ContextMenu ContextMenu
            {
                get { return null; }
            }

            [Browsable(false)]
            public override DataTemplate TreeViewTemplate
            {
                get { return LazyTreeViewTemplate.Value; }
            }

            // ReSharper disable once StaticMemberInGenericType
            private static readonly Lazy<DataTemplate> LazyTreeViewTemplate = new Lazy<DataTemplate>(CreateTemplate);

            private static DataTemplate CreateTemplate()
            {
                HierarchicalDataTemplate template = new HierarchicalDataTemplate
                {
                    DataType = TypeCache<TView>.Type,
                    ItemsSource = new Binding("BindableChilds")
                };

                FrameworkElementFactory textBlock = new FrameworkElementFactory(typeof(TextBlock));
                textBlock.SetBinding(TextBlock.TextProperty, new Binding("Title"));
                textBlock.SetBinding(TextBlock.ContextMenuProperty, new Binding("ContextMenu"));

                template.VisualTree = textBlock;
                return template;
            }

            public override string ToString()
            {
                return Title;
            }
        }

        private sealed class YkdFileView : View<YkdFileView, YkdFile>
        {
            public YkdFileView(YkdFile native)
                : base(native)
            {
            }

            protected override IEnumerable<View> EnumerateChilds()
            {
                yield return new YkdBlockView(Native.Background);
                foreach (YkdBlock block in Native.Blocks)
                    yield return new YkdBlockView(block);
                yield return new YkdResourcesView(Native.Resources);
            }

            [Category("Заголовок")]
            [DisplayName("Неизвестно 1")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int HeaderUnknown1
            {
                get { return Native.Header.Unknown1; }
                set { Native.Header.Unknown1 = value; }
            }

            [Category("Заголовок")]
            [DisplayName("Неизвестно 2")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(ByteUpDownEditor), typeof(ByteUpDownEditor))]
            public byte HeaderUnknown2
            {
                get { return Native.Header.Unknown2; }
                set { Native.Header.Unknown2 = value; }
            }

            [Category("Заголовок")]
            [DisplayName("Неизвестно 3")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(ByteUpDownEditor), typeof(ByteUpDownEditor))]
            public byte HeaderUnknown3
            {
                get { return Native.Header.Unknown3; }
                set { Native.Header.Unknown3 = value; }
            }

            [Category("Заголовок")]
            [DisplayName("Неизвестно 4")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(ByteUpDownEditor), typeof(ByteUpDownEditor))]
            public byte HeaderUnknown4
            {
                get { return Native.Header.Unknown4; }
                set { Native.Header.Unknown4 = value; }
            }

            [Category("Заголовок")]
            [DisplayName("Неизвестно 5")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(ByteUpDownEditor), typeof(ByteUpDownEditor))]
            public byte HeaderUnknown5
            {
                get { return Native.Header.Unknown5; }
                set { Native.Header.Unknown5 = value; }
            }

            public override string Title
            {
                get { return String.Format("{0} (Count: {1})", base.Title, Native.Blocks.Length); }
            }
        }

        private sealed class YkdBlockView : View<YkdBlockView, YkdBlock>
        {
            public YkdBlockView(YkdBlock native)
                : base(native)
            {
            }

            protected override IEnumerable<View> EnumerateChilds()
            {
                if (Native.ZeroTail != null)
                    yield return new YkdBlockOptionalTailView(Native.ZeroTail);

                foreach (YkdBlockEntry entry in Native.Entries)
                    yield return new YkdBlockEntryView(entry);
            }

            public override string Title
            {
                get { return String.Format("{0} (Count: {1})", base.Title, Native.Entries.Length); }
            }

            [Category("Заголовок")]
            [DisplayName("Тип")]
            [Description("Тип блока.")]
            public uint Type
            {
                get { return Native.Type; }
            }

            [Category("Заголовок")]
            [DisplayName("Индекс")]
            [Description("Нечто, похожее на уникальный номер блока.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int Index
            {
                get { return (int)Native.Index; }
                set { Native.Index = (uint)value; }
            }

            [Category("Заголовок")]
            [DisplayName("Связанный блок")]
            [Description("Индекс связанного блока.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int AssociatedIndex
            {
                get { return (int)Native.AssociatedIndex; }
                set { Native.AssociatedIndex = (uint)value; }
            }

            [Category("Заголовок")]
            [DisplayName("Неизвестно")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int Unknown4
            {
                get { return (int)Native.Unknown; }
                set { Native.Unknown = (uint)value; }
            }

            [Category("Отображение")]
            [DisplayName("Матрица преобразования")]
            [Description("Массив 4-байтовых чисел, описывающих трансформирование изображения.")]
            [Editor(typeof(WrapTextBoxEditor), typeof(WrapTextBoxEditor))]
            public String TransformationMatrix
            {
                get { return ReadInt32ArrayAsString(Native.TransformationMatrix); }
                set { WriteInt32ArrayFromString(Native.TransformationMatrix, value); }
            }

            [Category("Хвост")]
            [DisplayName("Хвост (Типы 5,6)")]
            [Description("Константный массив из 48 байт.")]
            [Editor(typeof(WrapTextBoxEditor), typeof(WrapTextBoxEditor))]
            public String Tail56
            {
                get { return ReadInt32ArrayAsString(Native.Tail56); }
                set { WriteInt32ArrayFromString(Native.Tail56, value); }
            }
        }

        private sealed class YkdBlockOptionalTailView : View<YkdBlockOptionalTailView, YkdBlockOptionalTail>
        {
            public YkdBlockOptionalTailView(YkdBlockOptionalTail native)
                : base(native)
            {
            }

            protected override IEnumerable<View> EnumerateChilds()
            {
                yield break;
            }

            [Category("Неизвестные")]
            [DisplayName("Неизвестно1")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int Unknown1
            {
                get { return Native.Unknown1; }
                set { Native.Unknown1 = value; }
            }

            [Category("Неизвестные")]
            [DisplayName("Неизвестно1")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int Unknown2
            {
                get { return Native.Unknown2; }
                set { Native.Unknown2 = value; }
            }

            [Category("Неизвестные")]
            [DisplayName("Неизвестно1")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int Unknown3
            {
                get { return Native.Unknown3; }
                set { Native.Unknown3 = value; }
            }

            [Category("Неизвестные")]
            [DisplayName("Неизвестно1")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int Unknown4
            {
                get { return Native.Unknown4; }
                set { Native.Unknown4 = value; }
            }
        }

        private sealed class YkdBlockOptionalTailsView : View<YkdBlockOptionalTailsView, YkdBlockOptionalTails>
        {
            public YkdBlockOptionalTailsView(YkdBlockOptionalTails native)
                : base(native)
            {
            }

            protected override IEnumerable<View> EnumerateChilds()
            {
                foreach (var tail in Native.Tails)
                    yield return new YkdBlockOptionalTailView(tail);
            }

            public override string Title
            {
                get { return String.Format("{0} (Count: {1})", base.Title, Native.Tails.Length); }
            }

            [Category("Неизвестные")]
            [DisplayName("Неизвестно1")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int Unknown1
            {
                get { return Native.Unknown1; }
                set { Native.Unknown1 = value; }
            }

            [Category("Неизвестные")]
            [DisplayName("Неизвестно1")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int Unknown2
            {
                get { return Native.Unknown2; }
                set { Native.Unknown2 = value; }
            }

            [Category("Неизвестные")]
            [DisplayName("Неизвестно1")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int Unknown3
            {
                get { return Native.Unknown3; }
                set { Native.Unknown3 = value; }
            }
        }

        private sealed class YkdBlockEntryView : View<YkdBlockEntryView, YkdBlockEntry>
        {
            public YkdBlockEntryView(YkdBlockEntry native)
                : base(native)
            {
            }

            protected override IEnumerable<View> EnumerateChilds()
            {
                foreach (YkdFrames frame in Native.Frames)
                    yield return new YkdFramesView(frame);
            }

            public override string Title
            {
                get { return String.IsNullOrEmpty(Native.Name) ? "<empty>" : Native.Name; }
            }

            [Category("Описание")]
            [DisplayName("Имя")]
            [Description("Имя события. Вероятно, не несёт смысловой нагрузки.")]
            [Editor(typeof(TextBoxEditor), typeof(TextBoxEditor))]
            public String Name
            {
                get { return Native.Name; }
                set { Native.Name = value; }
            }

            [Category("События")]
            [DisplayName("Неизвестно 1")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int OUnknown1
            {
                get { return Native.Offsets.Unknown1; }
                set { Native.Offsets.Unknown1 = value; }
            }

            [Category("События")]
            [DisplayName("Неизвестно 2")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int OUnknown2
            {
                get { return Native.Offsets.Unknown2; }
                set { Native.Offsets.Unknown2 = value; }
            }

            [Category("События")]
            [DisplayName("Анимация?")]
            [Description("Каким-то образом влияет на анимацию.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int OUnknown3
            {
                get { return Native.Offsets.Unknown3; }
                set { Native.Offsets.Unknown3 = value; }
            }
        }

        private sealed class YkdFramesView : View<YkdFramesView, YkdFrames>
        {
            public YkdFramesView(YkdFrames native)
                : base(native)
            {
            }

            protected override IEnumerable<View> EnumerateChilds()
            {
                foreach (YkdFrame frame in Native.Frames)
                    yield return new YkdFrameView(frame);
            }

            public override string Title
            {
                get { return String.Format("{0} (Count: {1})", base.Title, Native.Count); }
            }

            [Category("Заголовок")]
            [DisplayName("Неизвестно 1")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int HUnknown1
            {
                get { return Native.Unknown1; }
                set { Native.Unknown1 = value; }
            }

            [Category("Заголовок")]
            [DisplayName("Флаги?")]
            [Description("Разворачивает анимацию колебания курсора.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int HUnknown2
            {
                get { return Native.Unknown2; }
                set { Native.Unknown2 = value; }
            }

            [Category("Заголовок")]
            [DisplayName("Неизвестно 3")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int HUnknown3
            {
                get { return Native.Unknown3; }
                set { Native.Unknown3 = value; }
            }
        }

        private sealed class YkdFrameView : View<YkdFrameView, YkdFrame>
        {
            public YkdFrameView(YkdFrame native)
                : base(native)
            {
            }

            protected override IEnumerable<View> EnumerateChilds()
            {
                yield break;
            }

            public override string Title
            {
                get { return String.Format("{0} (X: {1}, Y:{2})", base.Title, Native.X, Native.Y); }
            }

            [Category("Расположение")]
            [DisplayName("X")]
            [Description("Координата на оси OX.")]
            [Editor(typeof(SingleUpDownEditor), typeof(SingleUpDownEditor))]
            public float X
            {
                get { return Native.X; }
                set { Native.X = value; }
            }

            [Category("Расположение")]
            [DisplayName("Y")]
            [Description("Координата на оси OY.")]
            [Editor(typeof(SingleUpDownEditor), typeof(SingleUpDownEditor))]
            public float Y
            {
                get { return Native.Y; }
                set { Native.Y = value; }
            }

            [Category("Неизвестно")]
            [DisplayName("Неизвестно 1")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int Unknown1
            {
                get { return Native.Unknown1; }
                set { Native.Unknown1 = value; }
            }

            [Category("Неизвестно")]
            [DisplayName("Неизвестно 2")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(SingleUpDownEditor), typeof(SingleUpDownEditor))]
            public float Unknown2
            {
                get { return Native.Unknown2; }
                set { Native.Unknown2 = value; }
            }

            [Category("Неизвестно")]
            [DisplayName("Неизвестно 3")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int Unknown3
            {
                get { return Native.Unknown3; }
                set { Native.Unknown3 = value; }
            }

            [Category("Неизвестно")]
            [DisplayName("Неизвестно 4")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int Unknown4
            {
                get { return Native.Unknown4; }
                set { Native.Unknown4 = value; }
            }

            [Category("Неизвестно")]
            [DisplayName("Неизвестно 5")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(SingleUpDownEditor), typeof(SingleUpDownEditor))]
            public float Unknown5
            {
                get { return Native.Unknown5; }
                set { Native.Unknown5 = value; }
            }

            [Category("Неизвестно")]
            [DisplayName("Неизвестно 6")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(SingleUpDownEditor), typeof(SingleUpDownEditor))]
            public float Unknown6
            {
                get { return Native.Unknown6; }
                set { Native.Unknown6 = value; }
            }
        }

        private sealed class YkdResourcesView : View<YkdResourcesView, YkdResources>
        {
            public YkdResourcesView(YkdResources native)
                : base(native)
            {
            }

            protected override IEnumerable<View> EnumerateChilds()
            {
                foreach (YkdResource resource in Native.Resources)
                    yield return YkdResourceView.FromResource(resource, this);
            }

            public override string Title
            {
                get { return String.Format("{0} (Count: {1})", base.Title, Native.Count); }
            }

            [Category("События")]
            [DisplayName("Неизвестно 1")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int OUnknown1
            {
                get { return Native.Offsets.Unknown1; }
                set { Native.Offsets.Unknown1 = value; }
            }

            [Category("События")]
            [DisplayName("Неизвестно 2")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int OUnknown2
            {
                get { return Native.Offsets.Unknown2; }
                set { Native.Offsets.Unknown2 = value; }
            }

            [Category("События")]
            [DisplayName("Анимация?")]
            [Description("Каким-то образом влияет на анимацию.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int OUnknown3
            {
                get { return Native.Offsets.Unknown3; }
                set { Native.Offsets.Unknown3 = value; }
            }
        }

        private abstract class YkdResourceView : View<YkdResourceView, YkdResource>
        {
            private readonly YkdResourcesView _parent;

            protected YkdResourceView(YkdResource native, YkdResourcesView parent)
                : base(native)
            {
                _parent = parent;
            }

            protected override IEnumerable<View> EnumerateChilds()
            {
                yield break;
            }

            public override string Title
            {
                get { return String.Format("{0:D3} {1} {2}", Native.Index, Native.Type, String.IsNullOrEmpty(Native.Name) ? "<empty>" : Native.Name); }
            }

            public override ContextMenu ContextMenu
            {
                get
                {
                    UiContextMenu menu = UiContextMenuFactory.Create();
                    menu.AddChild(UiMenuItemFactory.Create("Сдублировать", new YkdResourceViewDuplicateCommand(this, _parent)));
                    menu.AddChild(UiMenuItemFactory.Create("Удалить", new YkdResourceViewRemoveCommand(this, _parent)));
                    return menu;
                }
            }

            private class YkdResourceViewRemoveCommand : ICommand
            {
                private YkdResourceView _resource;
                private YkdResourcesView _collection;

                public YkdResourceViewRemoveCommand(YkdResourceView ykdResourceView, YkdResourcesView parent)
                {
                    _resource = ykdResourceView;
                    _collection = parent;
                }
                
                public bool CanExecute(object parameter)
                {
                    return true;
                }

                public void Execute(object parameter)
                {
                    if (MessageBox.Show(Application.Current.MainWindow, "Вы уверены, что хотите удалить этот ресурс?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                        return;

                    _collection.Native.Remove(_resource.Native);
                    _collection.RaisePropertyChanged("BindableChilds");
                }

                public event EventHandler CanExecuteChanged;
            }

            private class YkdResourceViewDuplicateCommand : ICommand
            {
                private YkdResourceView _resource;
                private YkdResourcesView _collection;

                public YkdResourceViewDuplicateCommand(YkdResourceView ykdResourceView, YkdResourcesView parent)
                {
                    _resource = ykdResourceView;
                    _collection = parent;
                }

                public bool CanExecute(object parameter)
                {
                    return true;
                }

                public void Execute(object parameter)
                {
                    _collection.Native.Duplicate(_resource.Native);
                    _collection.RaisePropertyChanged("BindableChilds");
                }

                public event EventHandler CanExecuteChanged;
            }

            [Category("Текстура")]
            [DisplayName("Индекс")]
            [Description("Какой-то номер.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int Index
            {
                get { return Native.Index; }
                set { Native.Index = value; }
            }

            [Category("Текстура")]
            [DisplayName("Название")]
            [Description("Название файла текстуры с расширением .txbh из этого же архива.")]
            [Editor(typeof(TextBoxEditor), typeof(TextBoxEditor))]
            public String Name
            {
                get { return Native.Name; }
                set { Native.Name = value; }
            }

            [Category("Текстура")]
            [DisplayName("Тип")]
            [Description("Тип ресурса.")]
            public String Type
            {
                get { return Native.Type.ToString(); }
            }

            public static YkdResourceView FromResource(YkdResource resource, YkdResourcesView parent)
            {
                switch (resource.Type)
                {
                    case YkdResourceViewportType.Empty:
                        return new EmptyYkdResourceView(resource, parent);
                    case YkdResourceViewportType.Fragment:
                        return new FragmentYkdResourceView(resource, parent);
                    case YkdResourceViewportType.Full:
                        return new FullYkdResourceView(resource, parent);
                    case YkdResourceViewportType.Extra:
                        return new ExtraYkdResourceView(resource, parent);
                    default:
                        throw new NotImplementedException(resource.Type.ToString());
                }
            }
        }

        private class EmptyYkdResourceView : YkdResourceView
        {
            public EmptyYkdResourceView(YkdResource native, YkdResourcesView parent)
                : base(native, parent)
            {
            }

            protected override IEnumerable<View> EnumerateChilds()
            {
                yield break;
            }

            private EmptyYkdResourceViewport Viewport
            {
                get { return (EmptyYkdResourceViewport)Native.Viewport; }
            }
        }

        private class FragmentYkdResourceView : YkdResourceView
        {
            public FragmentYkdResourceView(YkdResource native, YkdResourcesView parent)
                : base(native, parent)
            {
            }

            protected override IEnumerable<View> EnumerateChilds()
            {
                yield break;
            }

            private FragmentYkdResourceViewport Viewport
            {
                get { return (FragmentYkdResourceViewport)Native.Viewport; }
            }

            [Category("Текстура")]
            [DisplayName("X")]
            [Description("Координата X верхнего-левого угла фрагмента текстуры.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int SourceX
            {
                get { return Viewport.SourceX; }
                set { Viewport.SourceX = value; }
            }

            [Category("Текстура")]
            [DisplayName("Y")]
            [Description("Координата Y верхнего-левого угла фрагмента текстуры.")]
            public int SourceY
            {
                get { return Viewport.SourceY; }
                set { Viewport.SourceY = value; }
            }

            [Category("Текстура")]
            [DisplayName("Ширина")]
            [Description("Ширина фрагмента текстуры.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int SourceWidth
            {
                get { return Viewport.SourceWidth; }
                set { Viewport.SourceWidth = value; }
            }

            [Category("Текстура")]
            [DisplayName("Высота")]
            [Description("Высота фрагмента текстуры.")]
            public int SourceHeight
            {
                get { return Viewport.SourceHeight; }
                set { Viewport.SourceHeight = value; }
            }

            [Category("Отображение")]
            [DisplayName("Ширина")]
            [Description("Ширина отображения.")]
            public int ViewportWidth
            {
                get { return Viewport.ViewportWidth; }
                set { Viewport.ViewportWidth = value; }
            }

            [Category("Отображение")]
            [DisplayName("Высота")]
            [Description("Высота отображения.")]
            public int ViewportHeight
            {
                get { return Viewport.ViewportHeight; }
                set { Viewport.ViewportHeight = value; }
            }

            [Category("Отображение")]
            [DisplayName("Флаги")]
            [Description("Различные модификаторы отображения.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int Flags
            {
                get { return (int)Viewport.Flags; }
                set { Viewport.Flags = (YkdResourceFlags)value; }
            }

            [Category("Неизвестные")]
            [DisplayName("Неизвестно 5")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int Unknown5
            {
                get { return Viewport.Unknown5; }
                set { Viewport.Unknown5 = value; }
            }

            [Category("Градиент")]
            [DisplayName("Верхний-левый")]
            [Description("Цвет верхнего-левого угла отображения.")]
            [Editor(typeof(ColorEditor), typeof(ColorEditor))]
            public Color UpperLeftColor
            {
                get { return ColorsHelper.GetBGRA(Viewport.UpperLeftColor); }
                set { Viewport.UpperLeftColor = ColorsHelper.GetBGRA(value); }
            }

            [Category("Градиент")]
            [DisplayName("Нижний-левый")]
            [Description("Цвет нижнего-левого угла отображения.")]
            [Editor(typeof(ColorEditor), typeof(ColorEditor))]
            public Color BottomLeftColor
            {
                get { return ColorsHelper.GetBGRA(Viewport.BottomLeftColor); }
                set { Viewport.BottomLeftColor = ColorsHelper.GetBGRA(value); }
            }

            [Category("Градиент")]
            [DisplayName("Верхний-правый")]
            [Description("Цвет верхнего-правого угла отображения.")]
            [Editor(typeof(ColorEditor), typeof(ColorEditor))]
            public Color UpperRightColor
            {
                get { return ColorsHelper.GetBGRA(Viewport.UpperRightColor); }
                set { Viewport.UpperRightColor = ColorsHelper.GetBGRA(value); }
            }

            [Category("Градиент")]
            [DisplayName("Нижний-правый")]
            [Description("Цвет нижнего-правого угла отображения.")]
            [Editor(typeof(ColorEditor), typeof(ColorEditor))]
            public Color BottomRightColor
            {
                get { return ColorsHelper.GetBGRA(Viewport.BottomRightColor); }
                set { Viewport.BottomRightColor = ColorsHelper.GetBGRA(value); }
            }
        }

        private class FullYkdResourceView : YkdResourceView
        {
            public FullYkdResourceView(YkdResource native, YkdResourcesView parent)
                : base(native, parent)
            {
            }

            protected override IEnumerable<View> EnumerateChilds()
            {
                yield break;
            }

            private FullYkdResourceViewport Viewport
            {
                get { return (FullYkdResourceViewport)Native.Viewport; }
            }

            [Category("Отображение")]
            [DisplayName("Ширина")]
            [Description("Ширина отображения.")]
            public int ViewportWidth
            {
                get { return Viewport.ViewportWidth; }
                set { Viewport.ViewportWidth = value; }
            }

            [Category("Отображение")]
            [DisplayName("Высота")]
            [Description("Высота отображения.")]
            public int ViewportHeight
            {
                get { return Viewport.ViewportHeight; }
                set { Viewport.ViewportHeight = value; }
            }

            [Category("Градиент")]
            [DisplayName("Верхний-левый")]
            [Description("Цвет верхнего-левого угла отображения.")]
            [Editor(typeof(ColorEditor), typeof(ColorEditor))]
            public Color UpperLeftColor
            {
                get { return ColorsHelper.GetBGRA(Viewport.UpperLeftColor); }
                set { Viewport.UpperLeftColor = ColorsHelper.GetBGRA(value); }
            }

            [Category("Градиент")]
            [DisplayName("Нижний-левый")]
            [Description("Цвет нижнего-левого угла отображения.")]
            [Editor(typeof(ColorEditor), typeof(ColorEditor))]
            public Color BottomLeftColor
            {
                get { return ColorsHelper.GetBGRA(Viewport.BottomLeftColor); }
                set { Viewport.BottomLeftColor = ColorsHelper.GetBGRA(value); }
            }

            [Category("Градиент")]
            [DisplayName("Верхний-правый")]
            [Description("Цвет верхнего-правого угла отображения.")]
            [Editor(typeof(ColorEditor), typeof(ColorEditor))]
            public Color UpperRightColor
            {
                get { return ColorsHelper.GetBGRA(Viewport.UpperRightColor); }
                set { Viewport.UpperRightColor = ColorsHelper.GetBGRA(value); }
            }

            [Category("Градиент")]
            [DisplayName("Нижний-правый")]
            [Description("Цвет нижнего-правого угла отображения.")]
            [Editor(typeof(ColorEditor), typeof(ColorEditor))]
            public Color BottomRightColor
            {
                get { return ColorsHelper.GetBGRA(Viewport.BottomRightColor); }
                set { Viewport.BottomRightColor = ColorsHelper.GetBGRA(value); }
            }

            [Category("Отображение")]
            [DisplayName("Неизвестно 1")]
            [Description("Неизвестное значение.")]
            public int Unknown1
            {
                get { return Viewport.Unknown1; }
                set { Viewport.Unknown1 = value; }
            }

            [Category("Отображение")]
            [DisplayName("Неизвестно 2")]
            [Description("Неизвестное значение.")]
            public int Unknown2
            {
                get { return Viewport.Unknown2; }
                set { Viewport.Unknown2 = value; }
            }
        }

        private class ExtraYkdResourceView : YkdResourceView
        {
            public ExtraYkdResourceView(YkdResource native, YkdResourcesView parent)
                : base(native, parent)
            {
            }

            protected override IEnumerable<View> EnumerateChilds()
            {
                yield break;
            }

            private ExtraYkdResourceViewport Viewport
            {
                get { return (ExtraYkdResourceViewport)Native.Viewport; }
            }

            [Category("Текстура")]
            [DisplayName("X")]
            [Description("Координата X верхнего-левого угла фрагмента текстуры.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int SourceX
            {
                get { return Viewport.SourceX; }
                set { Viewport.SourceX = value; }
            }

            [Category("Текстура")]
            [DisplayName("Y")]
            [Description("Координата Y верхнего-левого угла фрагмента текстуры.")]
            public int SourceY
            {
                get { return Viewport.SourceY; }
                set { Viewport.SourceY = value; }
            }

            [Category("Текстура")]
            [DisplayName("Ширина")]
            [Description("Ширина фрагмента текстуры.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int SourceWidth
            {
                get { return Viewport.SourceWidth; }
                set { Viewport.SourceWidth = value; }
            }

            [Category("Текстура")]
            [DisplayName("Высота")]
            [Description("Высота фрагмента текстуры.")]
            public int SourceHeight
            {
                get { return Viewport.SourceHeight; }
                set { Viewport.SourceHeight = value; }
            }

            [Category("Отображение")]
            [DisplayName("Ширина")]
            [Description("Ширина отображения.")]
            public int ViewportWidth
            {
                get { return Viewport.ViewportWidth; }
                set { Viewport.ViewportWidth = value; }
            }

            [Category("Отображение")]
            [DisplayName("Высота")]
            [Description("Высота отображения.")]
            public int ViewportHeight
            {
                get { return Viewport.ViewportHeight; }
                set { Viewport.ViewportHeight = value; }
            }

            [Category("Отображение")]
            [DisplayName("Флаги")]
            [Description("Различные модификаторы отображения.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int Flags
            {
                get { return (int)Viewport.Flags; }
                set { Viewport.Flags = (YkdResourceFlags)value; }
            }

            [Category("Неизвестные")]
            [DisplayName("Неизвестно 5")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int Unknown5
            {
                get { return Viewport.Unknown5; }
                set { Viewport.Unknown5 = value; }
            }

            [Category("Градиент")]
            [DisplayName("Верхний-левый")]
            [Description("Цвет верхнего-левого угла отображения.")]
            [Editor(typeof(ColorEditor), typeof(ColorEditor))]
            public Color UpperLeftColor
            {
                get { return ColorsHelper.GetBGRA(Viewport.UpperLeftColor); }
                set { Viewport.UpperLeftColor = ColorsHelper.GetBGRA(value); }
            }

            [Category("Градиент")]
            [DisplayName("Нижний-левый")]
            [Description("Цвет нижнего-левого угла отображения.")]
            [Editor(typeof(ColorEditor), typeof(ColorEditor))]
            public Color BottomLeftColor
            {
                get { return ColorsHelper.GetBGRA(Viewport.BottomLeftColor); }
                set { Viewport.BottomLeftColor = ColorsHelper.GetBGRA(value); }
            }

            [Category("Градиент")]
            [DisplayName("Верхний-правый")]
            [Description("Цвет верхнего-правого угла отображения.")]
            [Editor(typeof(ColorEditor), typeof(ColorEditor))]
            public Color UpperRightColor
            {
                get { return ColorsHelper.GetBGRA(Viewport.UpperRightColor); }
                set { Viewport.UpperRightColor = ColorsHelper.GetBGRA(value); }
            }

            [Category("Градиент")]
            [DisplayName("Нижний-правый")]
            [Description("Цвет нижнего-правого угла отображения.")]
            [Editor(typeof(ColorEditor), typeof(ColorEditor))]
            public Color BottomRightColor
            {
                get { return ColorsHelper.GetBGRA(Viewport.BottomRightColor); }
                set { Viewport.BottomRightColor = ColorsHelper.GetBGRA(value); }
            }
        }
    }
}