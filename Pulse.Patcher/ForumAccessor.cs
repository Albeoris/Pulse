using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;

namespace Pulse.Patcher
{
    public sealed class ForumAccessor
    {
        private static readonly Uri SidCookieUri = new Uri("http://ff13.ffrtt.ru/");

        private readonly CookieContainer _cookies;
        private string _sessionId;

        public ForumAccessor()
        {
            _cookies = new CookieContainer();
        }

        public void Login(string userName, string password)
        {
            const string url = "http://ff13.ffrtt.ru/ucp.php?";

            HttpRequestParams parameters = new HttpRequestParams
            {
                {"mode", "login"}
            };

            HttpRequestParams data = new HttpRequestParams
            {
                {"username", userName},
                {"password", password},
                {"login", "Вход"},
                {"redirect", "./index.php?"}
            };

            HttpWebRequest request = CreatePostRequest(url + parameters.BuildUrl());
            data.SendRequest(request);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream responseStream = response.GetResponseStream())
            {
                StreamReader sr = new StreamReader(responseStream);
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (line != null && line.Contains("Закрытая разработка"))
                    {
                        _sessionId = FindCookieValue("phpbb3_2vxwe_sid");
                        return;
                    }
                }
            }

            throw new Exception("Ошибка входа... или недостаточно прав... или что-то ещё...");
        }

        public void Log(string subject, string message)
        {
            Random rnd = new Random();

            const string topicUrl = "http://ff13.ffrtt.ru/posting.php?mode=reply&f=9&t=22";

            HttpResponseInputParser inputParser = new HttpResponseInputParser(4) {"form_token", "creation_time", "lastclick", "topic_cur_post_id"};

            using (HttpClientHandler handler = new HttpClientHandler())
            {
                handler.CookieContainer = _cookies;
                using (HttpClient client = new HttpClient(handler))
                {
                    HttpResponseMessage result = client.GetAsync(topicUrl).Result;
                    Dictionary<string, string> info = inputParser.Parse(result);

                    byte[] empty = new byte[0];
                    using (MemoryStream ms = new MemoryStream(empty))
                    using (MultipartFormDataContent content = new MultipartFormDataContent("---------------------------" + (10000000000000 + rnd.Next(0, 2111014680))))
                    {
                        Dictionary<string, HttpContent> dic = new Dictionary<string, HttpContent>
                        {
                            {"subject", new StringContent(subject)},
                            {"addbbcode20", new StringContent("100")},
                            {"message", new StringContent(message)},
                            {"post", new StringContent("Отправить")},
                            {"attach_sig", new StringContent("on")},
                            {"fileupload", new StreamContent(ms)},
                            {"filecomment", new StringContent(string.Empty)},
                        };

                        foreach (KeyValuePair<string, string> pair in info)
                            dic.Add(pair.Key, new StringContent(pair.Value));

                        foreach (KeyValuePair<string, HttpContent> pair in dic)
                        {
                            pair.Value.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") {Name = '"' + pair.Key + '"'};
                            pair.Value.Headers.ContentType = null;
                        }

                        StreamContent uppload = (StreamContent)dic["fileupload"];
                        uppload.Headers.ContentDisposition.FileName = " ";
                        uppload.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                        content.Add(dic["subject"]);
                        content.Add(dic["addbbcode20"]);
                        content.Add(dic["message"]);
                        content.Add(dic["topic_cur_post_id"]);
                        content.Add(dic["lastclick"]);
                        content.Add(dic["post"]);
                        content.Add(dic["attach_sig"]);
                        content.Add(dic["creation_time"]);
                        content.Add(dic["form_token"]);
                        content.Add(dic["fileupload"]);
                        content.Add(dic["filecomment"]);

                        Thread.Sleep(1500); // Без таймаута не работает
                        client.PostAsync(topicUrl, content).Result.EnsureSuccessStatusCode();
                    }
                }
            }
        }

        public string ReadSecurityKey()
        {
            const string topicUrl = "http://ff13.ffrtt.ru/viewtopic.php?f=9&t=21";
            const string tag = "<div class=\"content\">«Этому наверняка есть какое-то естественное объяснение.";

            using (HttpClientHandler handler = new HttpClientHandler())
            {
                handler.CookieContainer = _cookies;
                using (HttpClient client = new HttpClient(handler))
                {
                    HttpResponseMessage result = client.GetAsync(topicUrl).Result;
                    using (Stream responseStream = result.Content.ReadAsStreamAsync().Result)
                    {
                        StreamReader sr = new StreamReader(responseStream);
                        while (!sr.EndOfStream)
                        {
                            string line = sr.ReadLine();
                            if (line == null)
                                continue;

                            if (line.Contains(tag))
                                return line;
                        }
                    }
                }
            }

            throw new Exception("Не удалось получить ключ безопасности.");
        }

        private HttpWebRequest CreatePostRequest(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            request.Host = "ff13.ffrtt.ru";
            request.Referer = "	http://ff13.ffrtt.ru/index.php";
            request.CookieContainer = _cookies;
            return request;
        }

        private HttpWebRequest CreateGetRequest(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            request.Host = "ff13.ffrtt.ru";
            request.Referer = "	http://ff13.ffrtt.ru/index.php";
            request.CookieContainer = _cookies;
            return request;
        }

        private string FindCookieValue(string key)
        {
            foreach (Cookie cookie in _cookies.GetCookies(SidCookieUri))
            {
                if (cookie.Name == key)
                    return cookie.Value;
            }

            return null;
        }
    }
}