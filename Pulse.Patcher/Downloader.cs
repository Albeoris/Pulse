using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Pulse.Patcher
{
    public sealed class Downloader
    {
        public static async Task Download(string fileName)
        {
            using (Stream output = File.Create(fileName))
                await Download(output);
        }

        private static async Task Download(Stream output)
        {
            const string rawArchiveLink = "http://rawgit.com/Albeoris/Pulse/master/Translate/FF13TranslationAlpha.ecp";

            using (HttpClient client = new HttpClient())
            using (Stream input = await client.GetStreamAsync(rawArchiveLink))
                await input.CopyToAsync(output);
        }
    }
}