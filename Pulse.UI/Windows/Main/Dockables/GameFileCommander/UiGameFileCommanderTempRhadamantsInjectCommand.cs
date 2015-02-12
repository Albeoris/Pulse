using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using Pulse.Core;
using Pulse.FS;

namespace Pulse.UI
{
    public sealed class UiGameFileCommanderTempRhadamantsInjectCommand : ICommand
    {
        private readonly Func<UiArchives> _archivesProvider;

        public event EventHandler CanExecuteChanged;

        public UiGameFileCommanderTempRhadamantsInjectCommand(Func<UiArchives> archivesProvider)
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
                if (MessageBox.Show("You're rhadamants from xentax.com?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes ||
                    MessageBox.Show("Are you sure?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes ||
                    MessageBox.Show("Did you make backup of game files?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                    return;

                UiArchives archives = _archivesProvider();
                if (archives == null)
                    return;

                string dir;
                using (CommonOpenFileDialog dlg = new CommonOpenFileDialog("Select the directory containing the new files..."))
                {
                    dlg.IsFolderPicker = true;
                    if (dlg.ShowDialog() != CommonFileDialogResult.Ok)
                        return;

                    dir = dlg.FileName;
                }

                Dictionary<string, Dictionary<string, string>> dic = new Dictionary<string, Dictionary<string, string>>(2);

                string[] files = Directory.GetFiles(dir, "txtres*.txt", SearchOption.AllDirectories);
                if (files.Length == 0)
                {
                    MessageBox.Show("Cannot find any txtres*.txt files.");
                    return;
                }

                foreach (string file in files)
                {
                    string name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(file.ToLower()));
                    string locale = name.Substring(name.Length - 2);
                    if (locale == "jp")
                        continue;

                    Dictionary<string, string> entries;
                    if (!dic.TryGetValue(locale, out entries))
                    {
                        entries = new Dictionary<string, string>(1024);
                        dic.Add(locale, entries);
                    }

                    using (FileStream source = File.OpenRead(file))
                    {
                        ZtrTextReader reader = new ZtrTextReader(source, TxtZtrFormatter.Instance);
                        ZtrFileEntry[] fileEntries = reader.Read(out name);
                        foreach (ZtrFileEntry entry in fileEntries)
                            entries[entry.Key] = entry.Value;
                    }
                }

                foreach (IArchiveListing archiveListing in archives.Select(a=>a.Listing).Order(ArchiveListingInjectComparer.Instance))
                {
                    ArchiveListing fullListing = (ArchiveListing)archiveListing;

                    ArchiveListing listing = new ArchiveListing(fullListing.Accessor) {FullListing = fullListing};
                    foreach (ArchiveEntry entry in fullListing)
                    {
                        foreach (string locale in dic.Keys)
                        {
                            if (entry.Name.EndsWith(locale + ".ztr", StringComparison.InvariantCultureIgnoreCase))
                            {
                                listing.Add(entry);
                                break;
                            }
                        }
                    }
                    if (listing.Accessor.Level > 0)// && listing.Count == 0)
                        continue;

                    ArchiveInjector injector = new ArchiveInjector(listing, null, entry => ProvideEntryInjector(entry, dic));
                    UiProgressWindow.Execute("”паковка файлов", injector, injector.Inject, UiProgressUnits.Bytes);
                }

                MessageBox.Show("All done!.. I hope...", "Done!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "ќшибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private IArchiveEntryInjector ProvideEntryInjector(ArchiveEntry entry, Dictionary<string, Dictionary<string, string>> dic)
        {
            foreach (string locale in dic.Keys)
            {
                if (!entry.Name.EndsWith(locale + ".ztr", StringComparison.InvariantCultureIgnoreCase))
                    continue;

                return new ArchiveEntryInjectorTempRhadamantsTxtToZtr(entry, dic[locale], InteractionService.TextEncoding.Provide().Encoding);
            }
            return null;
        }
    }
}