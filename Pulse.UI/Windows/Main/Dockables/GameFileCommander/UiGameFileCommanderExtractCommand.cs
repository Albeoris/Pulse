using System;
using System.Windows;
using System.Windows.Input;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiGameFileCommanderExtractCommand : ICommand
    {
        private readonly Func<UiArchives> _archivesProvider;

        public event EventHandler CanExecuteChanged;

        public UiGameFileCommanderExtractCommand(Func<UiArchives> archivesProvider)
        {
            _archivesProvider = Exceptions.CheckArgumentNull(archivesProvider, "archivesProvider");
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            try
            {
                UiArchives archives = _archivesProvider();
                if (archives == null)
                    return;

                UiGameFileCommanderSettingsWindow settingsDlg = new UiGameFileCommanderSettingsWindow(true);
                if (settingsDlg.ShowDialog() != true)
                    return;

                Wildcard wildcard = new Wildcard(settingsDlg.Wildcard, false);
                bool? conversion = settingsDlg.Convert;
                //string targetDir = InteractionService.WorkingLocation.Provide().ProvideExtractedDirectory();

                FileSystemExtractionTarget target = new FileSystemExtractionTarget();

                foreach (IUiLeafsAccessor accessor in archives.AccessToCheckedLeafs(wildcard, conversion, null))
                    accessor.Extract(target);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}