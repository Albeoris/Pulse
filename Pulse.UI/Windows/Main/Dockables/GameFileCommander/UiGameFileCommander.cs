using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiGameFileCommander : UiMainDockableControl
    {
        private UiTreeView _treeView;

        public UiGameFileCommander()
        {
            #region Construct

            Header = "Ресурсы игры";
            
            _treeView = UiTreeViewFactory.Create();
            {
                _treeView.ContextMenu = CreateTreeViewContextMenu();
                Content = _treeView;
            }

            #endregion

            Loaded += OnLoaded;
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
                foreach (UiArchiveTreeViewItem rootNode in UiArchiveTreeBuilder.Build(InteractionService.GamePath))
                    _treeView.Items.Add(rootNode);
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
                menu.AddChild(UiMenuItemFactory.Create("Распаковать", new ExtractCommand(_treeView)));
            }
            return menu;
        }

        private sealed class ExtractCommand : ICommand
        {
            private readonly UiTreeView _commandTreeView;

            public ExtractCommand(UiTreeView treeView)
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
                    UiArchiveTreeViewItem item = _commandTreeView.SelectedItem as UiArchiveTreeViewItem;
                    if (item == null)
                        return;

                    string targetDir;
                    using (CommonOpenFileDialog dlg = new CommonOpenFileDialog("Выберите каталог..."))
                    {
                        dlg.IsFolderPicker = true;
                        if (dlg.ShowDialog() != CommonFileDialogResult.Ok)
                            return;

                        targetDir = dlg.FileName;
                    }

                    ArchiveListing listing = item.CreateChildListing();
                    using (ArchiveExtractor extractor = new ArchiveExtractor(listing, targetDir))
                        UiProgressWindow.Execute("Распаковка файлов", extractor, extractor.Extract, UiProgressUnits.Bytes);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            public event EventHandler CanExecuteChanged;

            public void SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
            {
                EventHandler h = CanExecuteChanged;
                if (h != null)
                    h.Invoke(sender, e);
            }
        }
    }
}