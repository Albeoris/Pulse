using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace Pulse.Patcher
{
    public sealed class HttpResponseInputParser : List<string>
    {
        public HttpResponseInputParser()
        {
        }

        public HttpResponseInputParser(int capacity)
            : base(capacity)
        {
        }

        public HttpResponseInputParser(IEnumerable<string> collection)
            : base(collection)
        {
        }

        public Dictionary<string, string> Parse(HttpResponseMessage result)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>(Count);

            using (Stream responseStream = result.Content.ReadAsStreamAsync().Result)
            {
                StreamReader sr = new StreamReader(responseStream);
                while (!sr.EndOfStream && Count > 0)
                {
                    string line = sr.ReadLine();
                    if (line == null)
                        continue;

                    TryParse(line, dic);
                }
            }

            return dic;
        }

        private void TryParse(string line, Dictionary<string, string> dic)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                string key = this[i];
                string pattern = $"name=\"{key}\" value=\"";

                int tokenIndex = line.IndexOf(pattern, StringComparison.Ordinal);
                if (tokenIndex < 0)
                    continue;

                RemoveAt(i);
                tokenIndex += pattern.Length;
                int length = line.IndexOf('"', tokenIndex + 1) - tokenIndex;

                string value = line.Substring(tokenIndex, length);
                dic.Add(key, value);
            }
        }
    }
}