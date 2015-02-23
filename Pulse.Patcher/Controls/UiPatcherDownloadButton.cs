using System.Threading.Tasks;

namespace Pulse.Patcher
{
    public sealed class UiPatcherDownloadButton : UiProgressButton
    {
        private const string DownloadLabel = "Скачать";
        private const string DownloadingLabel = "Скачивание...";

        public UiPatcherDownloadButton()
        {
            Label = DownloadLabel;
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

                Maximum = await downloader.GetRemoteFileSize();

                await downloader.Download(PatcherService.ArchiveFileName);
            }
            finally
            {
                Label = DownloadLabel;
            }
        }

        private void OnDownloadProgress(long bytes)
        {
            Position += bytes;
        }
    }
}