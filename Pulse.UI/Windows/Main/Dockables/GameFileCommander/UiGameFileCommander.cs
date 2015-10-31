using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Pulse.Core;
using Pulse.UI.Interaction;

namespace Pulse.UI
{
    public sealed class UiGameFileCommander : UiMainDockableControl
    {
        private readonly UiGrid _grid;
        private readonly UiTreeView _treeView;
        private readonly UiListView _listView;
        private UiArchives _treeNodes;

        public UiGameFileCommander()
        {
            #region Construct

            Header = Lang.Dockable.GameFileCommander.Header;

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
                    _listView.SetValue(VirtualizingStackPanel.IsVirtualizingProperty, false);
                    _listView.KeyDown += OnListViewKeyDown;
                    //_listView.SetBinding(Selector.SelectedItemProperty, new Binding("ListViewSelectedItem") {Mode = BindingMode.OneWayToSource});
                    _listView.SelectionChanged += OnListViewSelectionChanged;
                    _listView.DataContext = this;
                    _grid.AddUiElement(_listView, 0, 2);
                }
            }

            Content = _grid;

            #endregion

            Loaded += OnLoaded;
        }

        protected override int Index
        {
            get { return 1; }
        }

        //Binding
        public UiNode ListViewSelectedItem
        {
            set { InteractionService.RaiseSelectedNodeChanged(value); }
        }

        private void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UiNode node = e.AddedItems.OfType<UiNode>().FirstOrDefault();
            InteractionService.RaiseSelectedNodeChanged(node);
        }

        private void OnListViewKeyDown(object sender, KeyEventArgs e)
        {
            UiNode selectedChild = (UiNode)_listView.SelectedItem;

            if (e.Key == Key.Back)
            {
                UiNode current = (UiNode)_treeView.SelectedItem;
                if (current == null)
                    return;    

                selectedChild.IsSelected = false;
                GoToParent(current);
            }
            else if (e.Key == Key.Enter)
            {
                GoToChild(selectedChild);
            }
        }

        private void GoToParent(UiNode current)
        {
            UiNode parent = current.Parent;
            if (parent == null)
                return;

            current.IsSelected = false;
            parent.IsExpanded = true;
            parent.IsSelected = true;

            _listView.SelectedItem = current;
            _listView.FocusSelectedItem();
        }

        private void GoToChild(UiNode child)
        {
            if (child == null || child.GetChilds().Length < 1)
                return;

            UiNode current = child.Parent;
            if (current != null)
                current.IsSelected = false;

            child.IsExpanded = true;
            child.IsSelected = true;

            while (child != null)
            {
                child.IsExpanded = true;
                child = child.Parent;
            }

            _listView.SelectedIndex = 0;
            _listView.FocusSelectedItem();
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
            return style;
        }

        private void OnListViewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListViewItem item = sender as ListViewItem;
            if (item == null)
                return;

            UiNode nodeOld = (UiNode)item.Content;
            GoToChild(nodeOld);
        }

        private static DataTemplate CreateArchiveListingTemplate(bool hierarchical)
        {
            DataTemplate template;
            if (hierarchical)
            {
                //template = new HierarchicalDataTemplate {DataType = typeof(UiArchiveNodeOld)};
                //((HierarchicalDataTemplate)template).ItemsSource = new Binding("OrderedHierarchyChilds");
                template = new HierarchicalDataTemplate {DataType = typeof(UiContainerNode)};
                ((HierarchicalDataTemplate)template).ItemsSource = new Binding("BindableHierarchyChilds");
            }
            else
            {
                //template = new DataTemplate {DataType = typeof(UiArchiveNodeOld)};
                template = new DataTemplate {DataType = typeof(UiNode)};
            }

            FrameworkElementFactory stackPanel = new FrameworkElementFactory(typeof(StackPanel));
            stackPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            FrameworkElementFactory checkbox = new FrameworkElementFactory(typeof(CheckBox));
            checkbox.SetValue(ToggleButton.IsCheckedProperty, new Binding("IsChecked") {Mode = BindingMode.TwoWay});
            checkbox.SetValue(ToggleButton.IsThreeStateProperty, true);
            stackPanel.AppendChild(checkbox);

            FrameworkElementFactory image = new FrameworkElementFactory(typeof(Image));
            image.SetValue(HeightProperty, 16d);
            image.SetValue(Image.SourceProperty, new Binding("Icon"));
            image.SetValue(MarginProperty, new Thickness(3));
            stackPanel.AppendChild(image);

            //FrameworkElementFactory icon = new FrameworkElementFactory(typeof(ContentPresenter));
            //icon.SetValue(ContentPresenter.ContentProperty, new Binding("Icon"));
            //icon.SetValue(MarginProperty, new Thickness(3));
            //stackPanel.AppendChild(icon);

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
                UiContainerNode item = (UiContainerNode)_treeView.SelectedItem;
                if (item != null)
                    _listView.ItemsSource = item.BindableChilds;
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(this, ex);
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
                UiHelper.ShowError(this, ex);
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
                UiHelper.ShowError(this, ex);
            }
        }

        private async Task RefreshContent(GameLocationInfo obj)
        {
            try
            {
                if (CheckAccess())
                {
                    _treeNodes = await obj.ArchivesTree;
                    _treeView.ItemsSource = _treeNodes;

                    SelectNode();

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
                UiHelper.ShowError(this, ex);
            }
        }

        private void SelectNode()
        {
            try
            {
                string selectedPath = InteractionService.Configuration.Provide().FileCommanderSelectedNodePath;
                if (selectedPath == null)
                    return;

                string[] names = selectedPath.Split('|');
                int index = names.Length - 1;

                IEnumerable<UiNode> current = _treeNodes;
                while (index >= 0)
                {
                    string name = names[index--];
                    UiNode node = current.FirstOrDefault(n => n.Name == name);
                    if (node == null)
                        break;

                    node.IsExpanded = true;
                    if (index == -1)
                        node.IsSelected = true;
                    else
                        current = node.GetChilds();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private UiContextMenu CreateTreeViewContextMenu()
        {
            UiContextMenu menu = UiContextMenuFactory.Create();
            {
                menu.AddChild(UiMenuItemFactory.Create(Lang.Dockable.GameFileCommander.Unpack, new UiGameFileCommanderExtractCommand(() => _treeNodes)));
                menu.AddChild(UiMenuItemFactory.Create(Lang.Dockable.GameFileCommander.Pack, new UiGameFileCommanderInjectCommand(() => _treeNodes)));
            }
            return menu;
        }
    }
}