using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;

namespace Common.Tools.HttpClient
{
    public class Client
    {
        public string GetResponse(string uri,string method=null, string paramstring = null)
        {
            string result;
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = (Check);
                var req = (HttpWebRequest) WebRequest.Create(uri);
                req.ProtocolVersion = HttpVersion.Version11;
                req.Method = string.IsNullOrEmpty(method) ?string.IsNullOrEmpty(paramstring) ? "GET" : "POST" : method;
                req.ContentType = "application/json";
                req.Accept = "application/json,text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
                req.UserAgent =
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.110 Safari/537.36";
                req.KeepAlive = false;
                req.Timeout = TimeOut;
                req.Headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.9,zh-TW;q=0.8,en;q=0.7");
                if (!string.IsNullOrEmpty(Token))
                {
                    req.Headers.Add(HttpRequestHeader.Authorization,Token);
                }
                if (!string.IsNullOrEmpty(paramstring))
                {
                    var sw = new StreamWriter(req.GetRequestStream());
                    sw.Write(paramstring);
                    sw.Close();
                }

                var res = (HttpWebResponse) req.GetResponse();
                result =
                    $"Client: Receive Response HTTP\r\nProtocolVersion:{res.ProtocolVersion}\r\n StatusCode:{res.StatusCode}\r\n StatusDescription:{res.StatusDescription}";
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    var stream = res.GetResponseStream();
                    if (stream != null)
                    {
                        var sr = new StreamReader(stream);
                        result = sr.ReadToEnd();
                        sr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                result = JsonConvert.SerializeObject(new HttpRes<object>()
                {
                    Success = false,
                    Msg = e.Message,
                    Data = string.Empty,
                });
            }

            return result;
        }

        private bool Check(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            return true;
        }

        public bool HttpUploadFile(string url, string file, string paramName, NameValueCollection nvc=null)
        {
            if(!File.Exists(file))return false;
            var fileInfo = new FileInfo(file);
            var contentType = fileInfo.Extension;

            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.ContentType = "multipart/form-data; boundary=" + boundary;
            req.Method = "POST";
            req.KeepAlive = true;
            req.Credentials = System.Net.CredentialCache.DefaultCredentials;

            Stream rs = req.GetRequestStream();

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";

            if (nvc != null)
            {
                foreach (string key in nvc.Keys)
                {
                    rs.Write(boundarybytes, 0, boundarybytes.Length);
                    string formitem = string.Format(formdataTemplate, key, nvc[key]);
                    byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                    rs.Write(formitembytes, 0, formitembytes.Length);
                }
            }
            
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, file, contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            var buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            try
            {
                var res = (HttpWebResponse)req.GetResponse();
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            

            return false;
        }


        public string Token { get; set; }

        public int TimeOut { get; set; } = 10 * 1000;

        private string FormatUrl(string url)
        {
            if (!url.StartsWith("http://")||!url.StartsWith("https://")) url = $"http://{url}";
            //if (!url.EndsWith("/")) url = $"{url}/";
            //if (!url.EndsWith("printer/")) url = $"{url}printer/";
            return url;
        }
    }
}