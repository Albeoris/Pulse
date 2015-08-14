using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Pulse.Patcher
{
    public sealed class Downloader
    {
        private readonly ManualResetEvent _cancelEvent;

        public event Action<long> DownloadProgress;

        public Downloader(ManualResetEvent cancelEvent)
        {
            _cancelEvent = cancelEvent;
        }

        public async Task<HttpFileInfo> GetRemoteFileInfo(string url)
        {
            HttpFileInfo result = new HttpFileInfo();

            if (_cancelEvent.WaitOne(0))
                return result;

            WebRequest request = WebRequest.Create(url);
            request.Method = "HEAD";

            using (WebResponse resp = await request.GetResponseAsync())
            {
                if (_cancelEvent.WaitOne(0))
                    return result;

                result.ReadFromResponse(url, resp);
                return result;
            }
        }

        public async Task Download(string url, string fileName)
        {
            if (_cancelEvent.WaitOne(0))
                return;

            using (Stream output = File.Create(fileName))
                await Download(url, output);
        }

        private async Task Download(String url, Stream output)
        {
            if (_cancelEvent.WaitOne(0))
                return;

            using (HttpClient client = new HttpClient())
            using (Stream input = await client.GetStreamAsync(url))
                await PatcherService.CopyAsync(input, output, _cancelEvent, DownloadProgress);
        }
    }
}