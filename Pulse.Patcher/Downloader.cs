using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Pulse.Core;

namespace Pulse.Patcher
{
    public sealed class Downloader
    {
        private const string RawArchiveLink = "http://rawgit.com/Albeoris/Pulse/master/Translate/FF13TranslationAlpha.ecp";

        private readonly ManualResetEvent _cancelEvent;

        public event Action<long> DownloadProgress;

        public Downloader(ManualResetEvent cancelEvent)
        {
            _cancelEvent = cancelEvent;
        }

        public async Task<long> GetRemoteFileSize()
        {
            if (_cancelEvent.WaitOne(0))
                return -1;

            WebRequest request = WebRequest.Create(RawArchiveLink);
            request.Method = "HEAD";

            using (WebResponse resp = await request.GetResponseAsync())
            {
                if (_cancelEvent.WaitOne(0))
                    return -1;

                string header = resp.Headers.Get("Content-Length");

                long contentLength;
                if (long.TryParse(header, NumberStyles.Integer, CultureInfo.InvariantCulture, out contentLength))
                    return contentLength;

                return -1;
            }
        }

        public async Task Download(string fileName)
        {
            if (_cancelEvent.WaitOne(0))
                return;

            using (Stream output = File.Create(fileName))
                await Download(output);
        }

        private async Task Download(Stream output)
        {
            if (_cancelEvent.WaitOne(0))
                return;

            using (HttpClient client = new HttpClient())
            using (Stream input = await client.GetStreamAsync(RawArchiveLink))
                await PatcherService.CopyAsync(input, output, _cancelEvent, DownloadProgress);
        }
    }
}