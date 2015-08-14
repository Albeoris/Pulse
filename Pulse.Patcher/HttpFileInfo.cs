using System;
using System.Net;

namespace Pulse.Patcher
{
    public sealed class HttpFileInfo
    {
        public string Url;
        public long ContentLength = -1;
        public DateTime? LastModified;

        public void ReadFromResponse(string url, WebResponse response)
        {
            Url = url;
            ContentLength = response.ContentLength;
            HttpWebResponse httpResponse = response as HttpWebResponse;
            if (httpResponse != null)
                LastModified = httpResponse.LastModified;
        }
    }
}