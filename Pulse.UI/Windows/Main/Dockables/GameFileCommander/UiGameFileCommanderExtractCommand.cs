using System;
using System.Windows;
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
}