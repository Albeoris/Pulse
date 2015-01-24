using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Pulse.Core;
using Pulse.Core.Components;
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
                bool convert = settingsDlg.Convert;
                string targetDir = InteractionService.WorkingLocation.Provide().ProvideExtractedDirectory();

                foreach (IArchiveListing listing in archives.EnumerateCheckedEntries(wildcard))
                {
                    XgrArchiveListing xgrArchiveListing = listing as XgrArchiveListing;
                    if (xgrArchiveListing != null)
                    {
                        XgrArchiveExtractor extractor = new XgrArchiveExtractor(xgrArchiveListing, targetDir, entry => CreateXgrEntryExtractor(entry, convert));
                        UiProgressWindow.Execute("Распаковка файлов", extractor, extractor.Extract, UiProgressUnits.Bytes);
                    }
                    else
                    {
                        ArchiveExtractor extractor = new ArchiveExtractor((ArchiveListing)listing, targetDir, entry => CreateEntryExtractor(entry, convert));
                        UiProgressWindow.Execute("Распаковка файлов", extractor, extractor.Extract, UiProgressUnits.Bytes);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private IArchiveEntryExtractor CreateEntryExtractor(ArchiveEntry entry, bool convert)
        {
            if (!convert)
                return ArchiveEntryExtractorUnpack.Instance;

            switch (PathEx.GetMultiDotComparableExtension(entry.Name))
            {
                case ".ztr":
                    return ArchiveEntryExtractorZtrToStrings.Instance;
            }

            return ArchiveEntryExtractorUnpack.Instance;
        }

        private IXgrArchiveEntryExtractor CreateXgrEntryExtractor(WpdEntry entry, bool convert)
        {
            if (!convert)
                return XgrArchiveEntryExtractorUnpack.Instance;

            switch (entry.Extension.ToLower())
            {
                case "txbh":
                    return XgrArchiveEntryExtractorTxbhToDds.Instance;
            }

            return XgrArchiveEntryExtractorUnpack.Instance;
        }
    }
}