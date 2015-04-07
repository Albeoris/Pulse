using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiGameFileCommanderInjectCommand : ICommand
    {
        private readonly Func<UiArchives> _archivesProvider;

        public event EventHandler CanExecuteChanged;

        public UiGameFileCommanderInjectCommand(Func<UiArchives> archivesProvider)
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

                UiGameFileCommanderSettingsWindow settingsDlg = new UiGameFileCommanderSettingsWindow(false);
                if (settingsDlg.ShowDialog() != true)
                    return;

                Wildcard wildcard = new Wildcard(settingsDlg.Wildcard, false);
                bool? compression = settingsDlg.Compression;
                bool? conversion = settingsDlg.Convert;

                UiInjectionManager manager = new UiInjectionManager();
                FileSystemInjectionSource source = new FileSystemInjectionSource();
                foreach (IUiLeafsAccessor accessor in archives.AccessToCheckedLeafs(wildcard, conversion, compression))
                    accessor.Inject(source, manager);
                manager.WriteListings();


                //foreach (IArchiveListing listing in archives.EnumerateCheckedEntries(wildcard).Order(ArchiveListingInjectComparer.Instance))
                //{
                //    XgrArchiveListing xgrArchiveListing = listing as XgrArchiveListing;
                //    if (xgrArchiveListing != null)
                //    {
                //        string archivePath = Path.Combine(sourceDir, Path.ChangeExtension(xgrArchiveListing.Name, ".unpack"));
                //        XgrArchiveInjector injector = new XgrArchiveInjector(xgrArchiveListing, compress, entry => ProvideXgrEntryInjector(entry, archivePath, convert));
                //        UiProgressWindow.Execute("”паковка файлов", injector, injector.Inject, UiProgressUnits.Bytes);
                //    }
                //    else
                //    {
                //        ArchiveInjector injector = new ArchiveInjector((ArchiveListing)listing, compress, entry => ProvideEntryInjector(entry, sourceDir, convert));
                //        UiProgressWindow.Execute("”паковка файлов", injector, injector.Inject, UiProgressUnits.Bytes);
                //    }
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "ќшибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //private IArchiveEntryInjector ProvideEntryInjector(ArchiveEntry entry, string sourceDir, bool convert)
        //{
        //    if (!convert)
        //        return ArchiveEntryInjectorPack.TryCreate(sourceDir, entry);

        //    IArchiveEntryInjector result = null;
        //    switch (PathEx.GetMultiDotComparableExtension(entry.Name))
        //    {
        //        case ".ztr":
        //            result = ArchiveEntryInjectorStringsToZtr.TryCreate(sourceDir, entry);
        //            break;
        //    }

        //    return result ?? ArchiveEntryInjectorPack.TryCreate(sourceDir, entry);
        //}

        //private IXgrArchiveEntryInjector ProvideXgrEntryInjector(WpdEntry entry, string sourceDir, bool convert)
        //{
        //    if (!convert)
        //        return XgrArchiveEntryInjectorPack.TryCreate(sourceDir, entry);

        //    IXgrArchiveEntryInjector result = null;
        //    switch (entry.Extension.ToLower())
        //    {
        //        case "txbh":
        //            result = XgrArchiveEntryInjectorDdsToTxb.TryCreate(sourceDir, entry);
        //            break;
        //    }

        //    return result ?? XgrArchiveEntryInjectorPack.TryCreate(sourceDir, entry);
        //}
    }
}