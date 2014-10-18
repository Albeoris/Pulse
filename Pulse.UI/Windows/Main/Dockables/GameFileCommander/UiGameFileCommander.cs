using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using Pulse.Core.Components;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiGameFileCommanderExtractCommand : ICommand
        {
            private readonly UiTreeView _commandTreeView;

            public UiGameFileCommanderExtractCommand(UiTreeView treeView)
            {
                _commandTreeView = treeView;
                _commandTreeView.SelectedItemChanged += SelectedItemChanged;
            }

            public bool CanExecute(object parameter)
            {
                return _commandTreeView.SelectedItem != null;
            }

            public void Execute(object parameter)
            {
                try
                {
                    UiArchiveNode item = _commandTreeView.SelectedItem as UiArchiveNode;
                    if (item == null)
                        return;

                    Wildcard wildcard = null;
                    if (item.Childs.Length > 0)
                    {
                        UiInputWindow dlg = new UiInputWindow("Укажите маску поиска...", "*");
                        if (dlg.ShowDialog() != true)
                            return;

                        wildcard = new Wildcard(dlg.Answer);
                    }

                    string targetDir;
                    using (CommonOpenFileDialog dlg = new CommonOpenFileDialog("Выберите каталог..."))
                    {
                        dlg.IsFolderPicker = true;
                        if (dlg.ShowDialog() != CommonFileDialogResult.Ok)
                            return;

                        targetDir = dlg.FileName;
                    }

                    ArchiveListing listing = item.CreateChildListing(wildcard);
                    ArchiveExtractor extractor = new ArchiveExtractor(listing, targetDir);
                    UiProgressWindow.Execute("Распаковка файлов", extractor, extractor.Extract, UiProgressUnits.Bytes);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            public event EventHandler CanExecuteChanged;

            private void SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
            {
                EventHandler h = CanExecuteChanged;
                if (h != null)
                    h.Invoke(sender, e);
            }
        }

    public sealed class UiGameFileCommander : UiMainDockableControl
    {
        private readonly UiGrid _grid;
        private readonly UiTreeView _treeView;
        private readonly UiListView _listView;
        private UiArchiveNode[] _treeNodes;

        public UiGameFileCommander()
        {
            #region Construct

            Header = "Ресурсы игры";

            _grid = UiGridFactory.Create(1, 3);
            {
                _grid.ColumnDefinitions[1].Width = GridLength.Auto;
                _grid.AddVerticalSplitter(1);

                _treeView = UiTreeViewFactory.Create();
                {
                    _treeView.ItemTemplate = CreateArchiveListingTemplate(true);
                    _treeView.ItemContainerStyle = CreateTreeViewItemContainerStyle();
                    _treeView.ContextMenu = CreateTreeViewContextMenu();
                    _treeView.SelectedItemChanged += OnTreeViewSelectedItemChanged;
                    _grid.AddUiElement(_treeView, 0, 0);
                }

                _listView = UiListViewFactory.Create();
                {
                    _listView.ItemTemplate = CreateArchiveListingTemplate(false);
                    _listView.ItemContainerStyle = CreateListViewItemContainerStyle();
                    _listView.KeyDown += OnListViewKeyDown;
                    //_listView.ContextMenu = contextMenut;
                    _grid.AddUiElement(_listView, 0, 2);
                }
            }

            Content = _grid;

            #endregion

            Loaded += OnLoaded;
        }

        private void OnListViewKeyDown(object sender, KeyEventArgs e)
        {
            UiArchiveNode node = (UiArchiveNode)_treeView.SelectedItem;
            if (node == null)
                return;

            if (e.Key == Key.Back)
            {
                UiArchiveNode parent = node.Parent.Parent;
                if (parent != null)
                {
                    parent.IsExpanded = true;
                    parent.IsSelected = true;
                }
                _listView.Focus();
            }
            else if (e.Key == Key.Enter)
            {
                node.IsExpanded = true;
                node.IsSelected = true;
                _listView.Focus();
            }
        }

        private Style CreateTreeViewItemContainerStyle()
        {
            Style style = new Style();
            style.Setters.Add(new Setter(TreeViewItem.IsSelectedProperty, new Binding("IsSelected"){Mode = BindingMode.TwoWay}));
            style.Setters.Add(new Setter(TreeViewItem.IsExpandedProperty, new Binding("IsExpanded"){Mode = BindingMode.TwoWay}));
            return style;
        }

        private Style CreateListViewItemContainerStyle()
        {
            Style style = new Style();
            style.Setters.Add(new EventSetter(TreeViewItem.MouseDoubleClickEvent, new MouseButtonEventHandler(OnListViewMouseDoubleClick)));
            style.Setters.Add(new Setter(TreeViewItem.IsSelectedProperty, new Binding("IsSelected"){Mode = BindingMode.TwoWay}));
            return style;
        }

        private void OnListViewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item == null)
                return;

            UiArchiveNode node = (UiArchiveNode)item.Header;
            if (node == null || node.Childs.Length < 1)
                return;

            node.IsSelected = true;
            
            while (node != null)
            {
                node.IsExpanded = true;
                node = node.Parent;
            }

            _listView.Focus();
        }

        private static DataTemplate CreateArchiveListingTemplate(bool hierarchical)
        {
            DataTemplate template;
            if (hierarchical)
            {
                template = new HierarchicalDataTemplate {DataType = typeof(UiArchiveNode)};
                ((HierarchicalDataTemplate)template).ItemsSource = new Binding("FolderChilds");
            }
            else
            {
                template = new DataTemplate {DataType = typeof(UiArchiveNode)};
            }

            FrameworkElementFactory stackPanel = new FrameworkElementFactory(typeof(StackPanel));
            stackPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            FrameworkElementFactory image = new FrameworkElementFactory(typeof(Image));
            image.SetValue(Image.SourceProperty, new Binding("Icon"));
            image.SetValue(Image.MarginProperty, new Thickness(3));
            stackPanel.AppendChild(image);

            FrameworkElementFactory textBlock = new FrameworkElementFactory(typeof(TextBlock));
            textBlock.SetBinding(TextBlock.TextProperty, new Binding("Name"));
            image.SetValue(TextBlock.MarginProperty, new Thickness(3));
            stackPanel.AppendChild(textBlock);

            template.VisualTree = stackPanel;

            return template;
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                UiArchiveNode item = (UiArchiveNode)_treeView.SelectedItem;
                if (item != null)
                    _listView.ItemsSource = item.Childs;
            }
            catch (Exception ex)
            {
                UIHelper.ShowError(ex);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            InteractionService.Refreshed += Refresh;
            Refresh();
        }

        private void Refresh()
        {
            try
            {
                _treeNodes = UiArchiveTreeBuilder.Build(InteractionService.GamePath);
                _treeView.ItemsSource = _treeNodes;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public override int Index
        {
            get { return 0; }
        }

        private UiContextMenu CreateTreeViewContextMenu()
        {
            UiContextMenu menu = UiContextMenuFactory.Create();
            {
                menu.AddChild(UiMenuItemFactory.Create("Распаковать", new UiGameFileCommanderExtractCommand(_treeView)));
            }
            return menu;
        }
    }
}