using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Pulse.Core;
using Pulse.UI;

namespace Pulse.Patcher
{
    public sealed class UiPatcherDownloadButton : UiProgressButton
    {
        private const string DownloadLabel = "Обновить";
        private const string DownloadingLabel = "Обновление...";

        private readonly Lazy<HttpFileInfo> _latestTranslationInfo;

        public UiPatcherDownloadButton()
        {
            Label = DownloadLabel;
            _latestTranslationInfo = new Lazy<HttpFileInfo>(GetLatestTranslationInfo);
            Task.Run(() => GetLatest());
        }

        private void GetLatest()
        {
            try
            {
                HttpFileInfo value = _latestTranslationInfo.Value;

                Dispatcher.Invoke(() =>
                {
                    if (value == null)
                    {
                        IsEnabled = false;
                        SubLabel = "Сервер недоступен";
                        return;
                    }

                    string server = value.Url;
                    int index = server.IndexOf("//");
                    if (index > -1)
                        server = server.Substring(index + 2);
                    
                    index = server.IndexOf('/');
                    if (index > -1)
                        server = server.Substring(0, index);

                    StringBuilder sb = new StringBuilder(128);
                    sb.Append(server);
                    sb.Append(" (");
                    sb.Append(UiHelper.FormatBytes(value.ContentLength));
                    if (value.LastModified != null)
                    {
                        sb.Append(", ");
                        sb.Append(value.LastModified);
                    }
                    sb.Append(')');
                    SubLabel = sb.ToString();

                    if (!File.Exists(PatcherService.ArchiveFileName) || File.GetLastWriteTime(PatcherService.ArchiveFileName) < value.LastModified)
                        SubLabelColor = Brushes.LightGreen;
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        protected override async Task DoAction()
        {
            Label = DownloadingLabel;
            try
            {
                if (CancelEvent.WaitOne(0))
                    return;

                Downloader downloader = new Downloader(CancelEvent);
                downloader.DownloadProgress += OnDownloadProgress;

                HttpFileInfo fileInfo = _latestTranslationInfo.Value;
                Maximum = fileInfo.ContentLength;

                await downloader.Download(fileInfo.Url, PatcherService.ArchiveFileName);

                if (fileInfo.LastModified != null)
                    File.SetLastWriteTime(PatcherService.ArchiveFileName, fileInfo.LastModified.Value);

                SubLabel = null;
            }
            finally
            {
                Label = DownloadLabel;
            }
        }

        private HttpFileInfo GetLatestTranslationInfo()
        {
            Downloader downloader = new Downloader(CancelEvent);
            LocalizatorEnvironmentInfo info = InteractionService.LocalizatorEnvironment.Provide();

            HttpFileInfo latest = null;
            foreach (string url in info.TranslationUrls)
            {
                try
                {
                    HttpFileInfo fileInfo = downloader.GetRemoteFileInfo(url).ContinueWith(t => t.Result).Result;
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

        private void OnDownloadProgress(long bytes)
        {
            Position += bytes;
        }
    }
}