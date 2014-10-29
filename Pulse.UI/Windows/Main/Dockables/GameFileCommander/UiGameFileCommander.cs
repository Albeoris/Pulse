using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Pulse.UI
{
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
            style.Setters.Add(new Setter(TreeViewItem.IsSelectedProperty, new Binding("IsSelected") {Mode = BindingMode.TwoWay}));
            style.Setters.Add(new Setter(TreeViewItem.IsExpandedProperty, new Binding("IsExpanded") {Mode = BindingMode.TwoWay}));
            return style;
        }

        private Style CreateListViewItemContainerStyle()
        {
            Style style = new Style();
            style.Setters.Add(new EventSetter(MouseDoubleClickEvent, new MouseButtonEventHandler(OnListViewMouseDoubleClick)));
            style.Setters.Add(new Setter(ListBoxItem.IsSelectedProperty, new Binding("IsSelected") {Mode = BindingMode.TwoWay}));
            return style;
        }

        private void OnListViewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListViewItem item = sender as ListViewItem;
            if (item == null)
                return;

            UiArchiveNode node = (UiArchiveNode)item.Tag;
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
            image.SetValue(MarginProperty, new Thickness(3));
            stackPanel.AppendChild(image);

            FrameworkElementFactory textBlock = new FrameworkElementFactory(typeof(TextBlock));
            textBlock.SetBinding(TextBlock.TextProperty, new Binding("Name"));
            textBlock.SetValue(MarginProperty, new Thickness(3));
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
                UiHelper.ShowError(ex);
            }
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await RefreshContent(InteractionService.GameLocation.Provide());
                InteractionService.GameLocation.InfoLost += ClearContent;
                InteractionService.GameLocation.InfoProvided += async v => await RefreshContent(v);
            }
            catch (Exception ex)
            {
                ClearContent();
                UiHelper.ShowError(ex);
            }
        }

        private void ClearContent()
        {
            try
            {
                if (CheckAccess())
                {
                    _treeView.ItemsSource = null;
                    _listView.ItemsSource = null;

                    IsEnabled = false;
                }
                else
                {
                    Dispatcher.Invoke(ClearContent);
                }
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(ex);
            }
        }

        private async Task RefreshContent(GameLocationInfo obj)
        {
            try
            {
                if (CheckAccess())
                {
                    _treeNodes = await UiArchiveTreeBuilder.BuildAsync(obj);
                    _treeView.ItemsSource = _treeNodes;

                    IsEnabled = true;
                }
                else
                {
                    await Dispatcher.Invoke(async () => await RefreshContent(obj));
                }
            }
            catch (Exception ex)
            {
                ClearContent();
                UiHelper.ShowError(ex);
            }
        }

        protected override int Index
        {
            get { return 1; }
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