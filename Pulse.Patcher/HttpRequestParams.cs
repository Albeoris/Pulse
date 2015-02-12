using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace Pulse.Patcher
{
    public sealed class HttpRequestParams : NameValueCollection
    {
        private const string SeparatorString = "=&";
        private static readonly byte[] SeparatorBytes = Encoding.ASCII.GetBytes(SeparatorString);
        
        public string BuildUrl()
        {
            StringBuilder sb = new StringBuilder(1024);
            foreach (string key in AllKeys)
            {
                sb.Append(HttpUtility.UrlEncode(key));
                sb.Append(SeparatorString[0]);
                sb.Append(HttpUtility.UrlEncode(this[key]));
                sb.Append(SeparatorString[1]);
            }
            sb.Length--;
            return sb.ToString();
        }

        public void SendRequest(HttpWebRequest request)
        {
            int index = 0, size = 0;
            byte[][] data = new byte[Count * 2][];
            foreach (string key in AllKeys)
            {
                data[index] = HttpUtility.UrlEncodeToBytes(key);
                size += data[index++].Length;

                data[index] = HttpUtility.UrlEncodeToBytes(this[key]);
                size += data[index++].Length;
            }

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = size + Count * 2 - 1;

            using (Stream requestStream = request.GetRequestStream())
            {
                for (int i = 0; i < data.Length; i++)
                {
                    byte[] buff = data[i];
                    requestStream.Write(buff, 0, buff.Length);
                    if (i < data.Length - 1)
                        requestStream.WriteByte(SeparatorBytes[i % 2]);
                }
            }
        }
    }
}