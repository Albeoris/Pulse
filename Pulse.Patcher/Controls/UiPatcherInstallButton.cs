using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using Pulse.Core;
using Pulse.FS;
using Pulse.UI;

namespace Pulse.Patcher
{
    public sealed class UiPatcherInstallButton : UiProgressButton
    {
        private const string InstallLabel = "Установить";
        private const string InstallationLabel = "Установка...";
        private const string DownloadingLabel = "Скачивание...";

        public UiPatcherInstallButton()
        {
            Label = InstallLabel;
        }

        protected override async Task DoAction()
        {
            Label = InstallationLabel;
            DirectoryInfo dir = null;
            try
            {
                dir = ExtractZipToTempFolder(PatcherService.ArchiveFileName);

                if (CancelEvent.IsSet())
                    return;

                await Patch(dir);
            }
            finally
            {
                Label = InstallLabel;
                if (dir != null)
                    dir.Delete(true);
            }
        }

        private void OnProgress(long value)
        {
            Position += value;
        }

        private async Task<string> DownloadLatestPatcher()
        {
            Label = DownloadingLabel;
            try
            {
                if (CancelEvent.WaitOne(0))
                    return null;

                Downloader downloader = new Downloader(CancelEvent);
                downloader.DownloadProgress += OnDownloadProgress;

                HttpFileInfo latest = await GetLatestPatcherUrl(downloader);
                if (latest == null)
                    throw new Exception("Не удалось найти свежую версию программы установки. Проверьте файл конфигурации.");

                Maximum = latest.ContentLength;

                string filePath = Path.GetTempFileName();
                await downloader.Download(latest.Url, filePath);
                return filePath;
            }
            finally
            {
                Label = InstallLabel;
            }
        }

        private async Task<HttpFileInfo> GetLatestPatcherUrl(Downloader downloader)
        {
            LocalizatorEnvironmentInfo info = InteractionService.LocalizatorEnvironment.Provide();

            HttpFileInfo latest = null;
            foreach (string url in info.PatherUrls)
            {
                try
                {
                    HttpFileInfo fileInfo = await downloader.GetRemoteFileInfo(url);
                    if (latest == null || latest.LastModified < fileInfo.LastModified)
                        latest = fileInfo;
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Не удалось получить информацию о файле: [{0}]", url);
                }
            }
            return latest;
        }

        private void OnDownloadProgress(long value)
        {
            Position += value;
        }

        private DirectoryInfo ExtractZipToTempFolder(string zipPath)
        {
            using (ZipArchive zipFile = ZipFile.OpenRead(zipPath))
            {
                if (CancelEvent.IsSet())
                    return null;

                DirectoryInfo dir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
                zipFile.ExtractToDirectory(dir.FullName);

                return dir;
            }
        }

        private async Task Patch(DirectoryInfo translationDir)
        {
            Position = 0;

            if (CancelEvent.IsSet())
                return;

            FFXIIIGamePart gamePart = FFXIIIGamePart.Part1; // TODO
            InteractionService.SetGamePart(gamePart);

            String configurationFilePath = Path.Combine(translationDir.FullName, PatcherService.ConfigurationFileName);
            XmlElement config = XmlHelper.LoadDocument(configurationFilePath);
            LocalizatorEnvironmentInfo info = LocalizatorEnvironmentInfo.FromXml(config["LocalizatorEnvironment"]);
            info.Validate();

            LocalizatorEnvironmentInfo currentInfo = InteractionService.LocalizatorEnvironment.Provide();
            currentInfo.UpdateUrls(info);

            InteractionService.LocalizatorEnvironment.SetValue(currentInfo);
            InteractionService.WorkingLocation.SetValue(new WorkingLocationInfo(translationDir.FullName));

            if (currentInfo.IsIncompatible(typeof(App).Assembly.GetName().Version))
            {
                if (MessageBox.Show(this.GetParentElement<Window>(), "Ваша версия программы установки несовместима с текущим перевод. Обновить?", "Ошибка!", MessageBoxButton.YesNo, MessageBoxImage.Error) != MessageBoxResult.Yes)
                    return;

                string path = await DownloadLatestPatcher();
                DirectoryInfo updatePath = ExtractZipToTempFolder(path);
                string destination = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');

                string patcherPath = Path.Combine(updatePath.FullName, "Pulse.Patcher.exe");
                ProcessStartInfo procInfo = new ProcessStartInfo(patcherPath, String.Format("/u \"{0}\"", destination))
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WorkingDirectory = updatePath.FullName
                };
                
                Process.Start(procInfo);
                Environment.Exit(0);
            }

            if (CancelEvent.IsSet())
                return;

            GameLocationInfo gameLocation = PatcherService.GetGameLocation(gamePart);
            await Task.Run(() => Patch(translationDir, gameLocation));
        }

        private void Patch(DirectoryInfo translationDir, GameLocationInfo gameLocation)
        {
            if (CancelEvent.IsSet())
                return;

            Dictionary<string, string> dic = ReadStrings(translationDir);
            if (CancelEvent.IsSet())
                return;

            FileSystemInjectionSource source = new FileSystemInjectionSource();
            source.RegisterStrings(dic);

            UiArchiveTreeBuilder builder = new UiArchiveTreeBuilder(gameLocation);
            UiArchives archives = builder.Build();
            Position = 0;
            Maximum = archives.Count;
            foreach (UiContainerNode archive in archives)
            {
                Check(archive);
                OnProgress(1);
            }

            if (CancelEvent.IsSet())
                return;

            IUiLeafsAccessor[] accessors = archives.AccessToCheckedLeafs(new Wildcard("*"), null, false).ToArray();
            Position = 0;
            Maximum = accessors.Length;

            UiInjectionManager manager = new UiInjectionManager();
            foreach (IUiLeafsAccessor accessor in accessors)
            {
                accessor.Inject(source, manager);
                OnProgress(1);
            }

            manager.WriteListings();
        }

        private void Check(UiNode node)
        {
            switch (node.Type)
            {
                case UiNodeType.Group:
                case UiNodeType.Directory:
                case UiNodeType.Archive:
                {
                    foreach (UiNode child in node.GetChilds())
                        Check(child);
                    break;
                }
                case UiNodeType.DataTable:
                {
                    if (PathComparer.Instance.Value.Equals(node.Name, @"system.win32.xgr") || node.Name.StartsWith("tutorial"))
                        foreach (UiNode child in node.GetChilds())
                            Check(child);
                    break;
                }
                case UiNodeType.ArchiveLeaf:
                {
                    UiArchiveLeaf leaf = (UiArchiveLeaf)node;
                    string extension = PathEx.GetMultiDotComparableExtension(leaf.Entry.Name);
                    switch (extension)
                    {
                        case ".ztr":
                            if (leaf.Entry.Name.Contains("_us"))
                                leaf.IsChecked = true;
                            break;
                    }
                    break;
                }
                case UiNodeType.DataTableLeaf:
                {
                    UiWpdTableLeaf leaf = (UiWpdTableLeaf)node;
                    switch (leaf.Entry.Extension)
                    {
                        case "wfl":
                        case "txbh":
                            leaf.IsChecked = true;
                            break;
                    }
                    break;
                }
            }
        }

        private Dictionary<string, string> ReadStrings(DirectoryInfo translationDir)
        {
            String[] strings = Directory.GetFiles(Path.Combine(translationDir.FullName, "Strings"), "*.strings", SearchOption.AllDirectories);

            Dictionary<string, string> result = new Dictionary<string, string>(16000);
            Maximum = strings.Length;

            for (int i = 0; i < strings.Length; i++)
            {
                if (CancelEvent.IsSet())
                    return result;

                using (FileStream input = File.OpenRead(strings[i]))
                {
                    string name;
                    ZtrTextReader unpacker = new ZtrTextReader(input, StringsZtrFormatter.Instance);
                    ZtrFileEntry[] entries = unpacker.Read(out name);

                    foreach (ZtrFileEntry entry in entries)
                    {
                        string key = entry.Key;
                        string value = entry.Value;
                        result[key] = value;
                    }
                }
            }

            return result;
        }
    }
}