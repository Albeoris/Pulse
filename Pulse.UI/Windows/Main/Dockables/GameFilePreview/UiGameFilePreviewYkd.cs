using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.WindowsAPICodePack.Dialogs;
using Pulse.Core;
using Pulse.FS;
using Pulse.OpenGL;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Pulse.UI
{
    public sealed class UiGameFilePreviewYkd : UiGrid
    {
        private readonly Tree _treeView;
        private readonly PropertyGrid _propertyGrid;
        private UiButton _rollbackButton;
        private UiButton _injectButton;
        private UiButton _saveAsButton;

        public UiGameFilePreviewYkd()
        {
            #region Constructor

            SetCols(2);
            SetRows(2);
            RowDefinitions[1].Height = new GridLength();

            _treeView = new Tree();
            _treeView.SelectedItemChanged += OnTreeViewSelectedItemChanged;
            AddUiElement(_treeView, 0, 0);

            _propertyGrid = new PropertyGrid {AutoGenerateProperties = true};
            AddUiElement(_propertyGrid, 0, 1);

            _rollbackButton = UiButtonFactory.Create("Отменить");
            {
                _rollbackButton.Width = 200;
                _rollbackButton.Margin = new Thickness(5);
                _rollbackButton.HorizontalAlignment = HorizontalAlignment.Left;
                _rollbackButton.VerticalAlignment = VerticalAlignment.Center;
                _rollbackButton.Click += OnRolbackButtonClick;
                AddUiElement(_rollbackButton, 1, 0, 0, 2);
            }

            _injectButton = UiButtonFactory.Create("Вставить");
            {
                _injectButton.Width = 200;
                _injectButton.Margin = new Thickness(5, 5, 210, 5);
                _injectButton.HorizontalAlignment = HorizontalAlignment.Right;
                _injectButton.VerticalAlignment = VerticalAlignment.Center;
                _injectButton.Click += OnInjectButtonClick;
                AddUiElement(_injectButton, 1, 0, 0, 2);
            }

            _saveAsButton = UiButtonFactory.Create("Сохранить как...");
            {
                _saveAsButton.Width = 200;
                _saveAsButton.Margin = new Thickness(5);
                _saveAsButton.HorizontalAlignment = HorizontalAlignment.Right;
                _saveAsButton.VerticalAlignment = VerticalAlignment.Center;
                _saveAsButton.Click += OnSaveAsButtonClick;
                AddUiElement(_saveAsButton, 1, 0, 0, 2);
            }

            #endregion
        }

        private WpdArchiveListing _listing;
        private WpdEntry _entry;
        private YkdFile _ykdFile;

        public void Show(WpdArchiveListing listing, WpdEntry entry)
        {
            _listing = listing;
            _entry = entry;

            if (listing == null || entry == null)
            {
                _treeView.ItemsSource = null;
                return;
            }

            using (Stream headers = listing.Accessor.ExtractHeaders())
            {
                _ykdFile = new StreamSegment(headers, entry.Offset, entry.Length, FileAccess.Read).ReadContent<YkdFile>();
                _treeView.ItemsSource = new[] {new YkdFileView(_ykdFile)};
            }

            Visibility = Visibility.Visible;
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

        private abstract class View
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
        }

        private abstract class View<TView, TNative> : View
        {
            protected readonly TNative Native;

            protected View(TNative native)
            {
                Native = native;
            }

            // Binding
            public virtual String Title
            {
                get { return TypeCache<TNative>.Type.Name; }
            }

            [Browsable(false)]
            public override DataTemplate TreeViewTemplate
            {
                get { return LazyTreeViewTemplate.Value; }
            }

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

                template.VisualTree = textBlock;
                return template;
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
        }

        private sealed class YkdBlockView : View<YkdBlockView, YkdBlock>
        {
            public YkdBlockView(YkdBlock native)
                : base(native)
            {
            }

            protected override IEnumerable<View> EnumerateChilds()
            {
                foreach (YkdBlockEntry entry in Native.Entries)
                    yield return new YkdBlockEntryView(entry);
            }

            public override string Title
            {
                get { return String.Format("{0} (Count: {1})", base.Title, Native.Entries.Length); }
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
            [DisplayName("Неизвестно 2")]
            [Description("Неизвестное значение.")]
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
                    yield return new YkdResourceView(resource);
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

        private sealed class YkdResourceView : View<YkdResourceView, YkdResource>
        {
            public YkdResourceView(YkdResource native)
                : base(native)
            {
            }

            protected override IEnumerable<View> EnumerateChilds()
            {
                yield break;
            }

            public override string Title
            {
                get { return String.Format("{0:D3} {1} (Size: {2})", Native.Index, String.IsNullOrEmpty(Native.Name) ? "<empty>" : Native.Name, Native.Size); }
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

            [Category("Неизвестные")]
            [DisplayName("Неизвестно 1")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int Unknown1
            {
                get { return Native.Unknown1; }
                set { Native.Unknown1 = value; }
            }

            [Category("Текстура")]
            [DisplayName("Координата X")]
            [Description("Координата X верхнего-левого угла фрагмента текстуры.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int SourceX
            {
                get { return Native.SourceX; }
                set { Native.SourceX = value; }
            }

            [Category("Текстура")]
            [DisplayName("Исходная Y")]
            [Description("Координата Y верхнего-левого угла фрагмента текстуры.")]
            public int SourceY
            {
                get { return Native.SourceY; }
                set { Native.SourceY = value; }
            }

            [Category("Текстура")]
            [DisplayName("Ширина")]
            [Description("Ширина фрагмента текстуры.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int SourceWidth
            {
                get { return Native.SourceWidth; }
                set { Native.SourceWidth = value; }
            }

            [Category("Текстура")]
            [DisplayName("Высота")]
            [Description("Высота фрагмента текстуры.")]
            public int SourceHeight
            {
                get { return Native.SourceHeight; }
                set { Native.SourceHeight = value; }
            }

            [Category("Отображение")]
            [DisplayName("Ширина")]
            [Description("Ширина отображения.")]
            public int ViewportWidth
            {
                get { return Native.ViewportWidth; }
                set { Native.ViewportWidth = value; }
            }

            [Category("Отображение")]
            [DisplayName("Высота")]
            [Description("Высота отображения.")]
            public int ViewportHeight
            {
                get { return Native.ViewportHeight; }
                set { Native.ViewportHeight = value; }
            }

            [Category("Неизвестные")]
            [DisplayName("Неизвестно 4")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int Unknown4
            {
                get { return Native.Unknown4; }
                set { Native.Unknown4 = value; }
            }

            [Category("Неизвестные")]
            [DisplayName("Неизвестно 5")]
            [Description("Неизвестное значение.")]
            [Editor(typeof(IntegerUpDownEditor), typeof(IntegerUpDownEditor))]
            public int Unknown5
            {
                get { return Native.Unknown5; }
                set { Native.Unknown5 = value; }
            }

            [Category("Градиент")]
            [DisplayName("Верхний-левый")]
            [Description("Цвет верхнего-левого угла представления.")]
            [Editor(typeof(ColorEditor), typeof(ColorEditor))]
            public Color UpperLeftColor
            {
                get { return ColorsHelper.GetBGRA(Native.UpperLeftColor); }
                set { Native.UpperLeftColor = ColorsHelper.GetBGRA(value); }
            }

            [Category("Градиент")]
            [DisplayName("Нижний-левый")]
            [Description("Цвет нижнего-левого угла представления.")]
            [Editor(typeof(ColorEditor), typeof(ColorEditor))]
            public Color BottomLeftColor
            {
                get { return ColorsHelper.GetBGRA(Native.BottomLeftColor); }
                set { Native.BottomLeftColor = ColorsHelper.GetBGRA(value); }
            }

            [Category("Градиент")]
            [DisplayName("Верхний-правый")]
            [Description("Цвет верхнего-правого угла представления.")]
            [Editor(typeof(ColorEditor), typeof(ColorEditor))]
            public Color UpperRightColor
            {
                get { return ColorsHelper.GetBGRA(Native.UpperRightColor); }
                set { Native.UpperRightColor = ColorsHelper.GetBGRA(value); }
            }

            [Category("Градиент")]
            [DisplayName("Нижний-правый")]
            [Description("Цвет нижнего-правого угла представления.")]
            [Editor(typeof(ColorEditor), typeof(ColorEditor))]
            public Color BottomRightColor
            {
                get { return ColorsHelper.GetBGRA(Native.BottomRightColor); }
                set { Native.BottomRightColor = ColorsHelper.GetBGRA(value); }
            }
        }
    }
}