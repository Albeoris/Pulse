using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Xml;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Pulse.Core;
using Pulse.FS;
using Pulse.UI;

namespace Pulse.Patcher
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string SteamRegistyPart1Path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 292120";
        private const string SteamRegistyPart2Path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 292140";
        private const string SteamGamePathTag = @"InstallLocation";

        private const PatchFormatVersion Version = PatchFormatVersion.V1;

        public MainWindow()
        {
            InitializeComponent();
        }

        private string GetSecurityKey(bool log)
        {
            ForumAccessor forumAccessor = new ForumAccessor();
            forumAccessor.Login(NameTextBox.Text, PasswordBox.Password);
            if (log)
                forumAccessor.Log("Установка патча", "Коварно пытаюсь поставить патч.");
            return forumAccessor.ReadSecurityKey();
        }

        private void OnPrepareButtonClick(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;

            try
            {
                string securityKey = GetSecurityKey(false);

                string root, targetPath;
                using (CommonOpenFileDialog dlg = new CommonOpenFileDialog("Выберите каталог..."))
                {
                    dlg.IsFolderPicker = true;
                    if (dlg.ShowDialog() != CommonFileDialogResult.Ok)
                        return;

                    root = dlg.FileName;
                }

                using (CommonSaveFileDialog dlg = new CommonSaveFileDialog("Сохранить как..."))
                {
                    dlg.DefaultFileName = "FF13TranslationAlpha.ecp";
                    if (dlg.ShowDialog() != CommonFileDialogResult.Ok)
                        return;

                    targetPath = dlg.FileName;
                }

                using (MemoryStream buff = new MemoryStream(16 * 1024 * 1024))
                {
                    BinaryWriter bw = new BinaryWriter(buff);
                    bw.Write((byte)PatchFormatVersion.V1);
                    bw.Write((byte)FFXIIIGamePart.Part1);

                    PackTexts(root, bw);

                    buff.Flush();
                    int length = (int)buff.Position;
                    buff.Position = 0;

                    using (MemoryStream compressed = new MemoryStream(length + 4 + 256))
                    using (BinaryWriter cbw = new BinaryWriter(compressed))
                    {
                        cbw.Write(length);
                        ZLibHelper.Compress(buff, compressed, length);
                        compressed.Position = 0;

                        using (CryptoProvider cryptoProvider = new CryptoProvider(securityKey))
                        using (FileStream output = File.Create(targetPath))
                            cryptoProvider.Encrypt(compressed, output);
                    }
                }

                MessageBox.Show(this, "Успех!");
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(ex);
            }
            finally
            {
                IsEnabled = true;
            }
        }

        private static void PackEncoding(string root, BinaryWriter bw)
        {
            string path = Path.Combine(root, "TextEncoding.xml");

            using (FileStream input = File.OpenRead(path))
            {
                bw.Write((int)input.Length);
                input.CopyTo(bw.BaseStream);
            }
        }

        private static void PackFonts(string root, BinaryWriter bw)
        {
            const string xgrArchiveName = @"gui/resident/system.win32.xgr";
            root = Path.Combine(root, @"Fonts");
            XgrPatchData.WriteTo(xgrArchiveName, root, bw);
        }

        private static void PackTexts(string root, BinaryWriter bw)
        {
            PackEncoding(root, bw);
            PackFonts(root, bw);
            PackStrings(root, bw);
        }

        private static void PackStrings(string root, BinaryWriter bw)
        {
            root = Path.Combine(root, @"Strings");

            foreach (string file in Directory.EnumerateFiles(root, "*.strings", SearchOption.AllDirectories))
            {
                using (FileStream input = File.OpenRead(file))
                {
                    string name;
                    ZtrTextReader reader = new ZtrTextReader(input, StringsZtrFormatter.Instance);
                    ZtrFileEntry[] entries = reader.Read(out name);
                    bw.Write(entries.Length);
                    foreach (ZtrFileEntry entry in entries)
                    {
                        bw.Write(entry.Key);
                        bw.Write(entry.Value);
                    }
                }
            }
            
            bw.Write(0);
        }

        private void OnInstallButtonClick(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;

            try
            {
                string filePath;
                using (CommonOpenFileDialog dlg = new CommonOpenFileDialog("Открыть..."))
                {
                    dlg.IsFolderPicker = false;
                    dlg.EnsureFileExists = false;
                    dlg.EnsurePathExists = true;
                    dlg.DefaultFileName = "ff13.ffrtt.ru";

                    if (dlg.ShowDialog() != CommonFileDialogResult.Ok)
                        return;

                    filePath = dlg.FileName;
                }

                using (SafeUnmanagedArray buff = Decompress(filePath))
                using (Stream input = buff.OpenStream(FileAccess.Read))
                    Patch(input);

                MessageBox.Show(this, "Установка успешно завершена!", "Повезло!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                UiHelper.ShowError(ex);
            }
            finally
            {
                IsEnabled = true;
            }
        }

        private void Patch(Stream input)
        {
            BinaryReader br = new BinaryReader(input);

            PatchFormatVersion fileVersion = (PatchFormatVersion)input.ReadByte();
            if (fileVersion > Version)
                throw new NotSupportedException("Пожалуйста, обновите программу установки.");

            FFXIIIGamePart gamePart = (FFXIIIGamePart)input.ReadByte();
            InteractionService.SetGamePart(gamePart);

            GameLocationInfo gameLocation = GetGameLocation(gamePart);
            PatchText(br, gameLocation);
        }

        private static FFXIIITextEncoding ReadEncoding(BinaryReader br)
        {
            int length = br.ReadInt32();
            using (MemoryStream ms = new MemoryStream(length))
            {
                byte[] buff = new byte[4096];
                br.BaseStream.CopyTo(ms, length, buff);
                ms.Position = 0;

                XmlDocument doc = new XmlDocument();
                doc.Load(ms);

                FFXIIICodePage codepage = FFXIIICodePageHelper.FromXml(doc.GetDocumentElement());
                return new FFXIIITextEncoding(codepage);
            }
        }

        private Dictionary<string, string> ReadStrings(BinaryReader br)
        {
            Dictionary<string, string> result = new Dictionary<string, string>(16000);

            for (int count = br.ReadInt32(); count > 0; count = br.ReadInt32())
            {
                for (int i = 0; i < count; i++)
                {
                    string key = br.ReadString();
                    string value = br.ReadString();
                    result[key] = value;
                }
            }

            return result;
        }

        private SafeUnmanagedArray Decompress(string filePath)
        {
            string securityKey = GetSecurityKey(true);

            using (FileStream input = File.OpenRead(filePath))
            using (MemoryStream ms = new MemoryStream((int)input.Length))
            {
                using (CryptoProvider cryptoProvider = new CryptoProvider(securityKey))
                    cryptoProvider.Decrypt(input, ms);

                ms.Position = 0;
                BinaryReader br = new BinaryReader(ms);
                int uncompressedSize = br.ReadInt32();
                byte[] buff = new byte[Math.Min(32 * 1024, uncompressedSize)];

                SafeUnmanagedArray result = new SafeUnmanagedArray(uncompressedSize);
                try
                {
                    using (UnmanagedMemoryStream output = result.OpenStream(FileAccess.Write))
                        ZLibHelper.Uncompress(ms, output, uncompressedSize, buff, CancellationToken.None);
                }
                catch
                {
                    result.SafeDispose();
                    throw;
                }

                return result;
            }
        }

        private GameLocationInfo GetGameLocation(FFXIIIGamePart gamePart)
        {
            try
            {
                using (RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                using (RegistryKey registryKey = localMachine.OpenSubKey(GetSteamRegistyPath(gamePart)))
                {
                    if (registryKey == null)
                        throw Exceptions.CreateException("Запись в реестре не обнаружена.");

                    GameLocationInfo result = new GameLocationInfo((string)registryKey.GetValue(SteamGamePathTag));
                    result.Validate();

                    return result;
                }
            }
            catch
            {
                using (CommonOpenFileDialog dlg = new CommonOpenFileDialog(String.Format("Укажите каталог Final Fantasy XIII-{0}...", (int)gamePart)))
                {
                    dlg.IsFolderPicker = true;
                    if (dlg.ShowDialog() != CommonFileDialogResult.Ok)
                        throw new OperationCanceledException();

                    GameLocationInfo result = new GameLocationInfo(dlg.FileName);
                    result.Validate();

                    return result;
                }
            }
        }

        private static string GetSteamRegistyPath(FFXIIIGamePart gamePart)
        {
            switch (gamePart)
            {
                case FFXIIIGamePart.Part1:
                    return SteamRegistyPart1Path;
                case FFXIIIGamePart.Part2:
                    return SteamRegistyPart2Path;
                default:
                    throw new NotImplementedException();
            }
        }

        private void PatchText(BinaryReader br, GameLocationInfo gameLocation)
        {
            FFXIIITextEncoding encoding = ReadEncoding(br);
            XgrPatchData fonts = XgrPatchData.ReadFrom(br);
            Dictionary<string, string> dic = ReadStrings(br);

            UiArchives archives = UiArchiveTreeBuilder.BuildAsync(gameLocation).Result;
            foreach (UiArchiveNode archive in archives)
                archive.IsChecked = true;

            foreach (IArchiveListing listing in archives.EnumerateCheckedEntries(new Wildcard("*")).Order(ArchiveListingInjectComparer.Instance))
            {
                XgrArchiveListing xgrArchiveListing = listing as XgrArchiveListing;
                if (xgrArchiveListing != null)
                {
                    if (xgrArchiveListing.Accessor.Name != fonts.XgrArchiveName)
                        continue;

                    XgrArchiveInjector injector = new XgrArchiveInjector(xgrArchiveListing, false, entry => ProvideXgrEntryInjector(entry, fonts));
                    UiProgressWindow.Execute("Упаковка файлов", injector, injector.Inject, UiProgressUnits.Bytes);
                }
            }

            foreach (IArchiveListing listing in archives.EnumerateCheckedEntries(new Wildcard("*us.ztr")).Order(ArchiveListingInjectComparer.Instance))
            {
                XgrArchiveListing xgrArchiveListing = listing as XgrArchiveListing;
                if (xgrArchiveListing != null)
                    continue;

                ArchiveInjector injector = new ArchiveInjector((ArchiveListing)listing, false, entry => ProvideEntryInjector(entry, dic, encoding));
                UiProgressWindow.Execute("Упаковка файлов", injector, injector.Inject, UiProgressUnits.Bytes);
            }
        }

        private IXgrArchiveEntryInjector ProvideXgrEntryInjector(WpdEntry entry, XgrPatchData fonts)
        {
            SafeUnmanagedArray data;
            switch (entry.Extension.ToLower())
            {
                case "txbh":
                {
                    if (fonts.TryGetValue(entry.Name + ".dds", out data))
                        return new XgrArchiveEntryInjectorDdsToTxb(data.OpenStream(FileAccess.Read), entry);
                    break;
                }
            }

            if (fonts.TryGetValue(entry.Name + '.' + entry.Extension, out data))
                return new XgrArchiveEntryInjectorPack(data.OpenStream(FileAccess.Read), entry);

            return null;
        }

        private IArchiveEntryInjector ProvideEntryInjector(ArchiveEntry entry, Dictionary<string, string> dic, FFXIIITextEncoding encoding)
        {
            switch (PathEx.GetMultiDotComparableExtension(entry.Name))
            {
                case ".ztr":
                    return new ArchiveEntryInjectorTempRhadamantsTxtToZtr(entry, dic, encoding);
            }

            return null;
        }
    }
}