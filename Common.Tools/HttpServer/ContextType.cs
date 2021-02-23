using System;

namespace Common.Tools.HttpServer
{
    public static class ContextType
    {
        public static string GetContextTypeByUri(Uri uri)
        {
            var ctype = "text/html";
            try
            {
                var uristring = uri.ToString();

                var index = uristring.IndexOf('?');
                var baseUri = index > 0 ? uristring.Remove(index) : uristring;

                index = baseUri.LastIndexOf('/');
                var fileName = index > 0 ? baseUri.Remove(0,index) : baseUri;

                index = fileName.LastIndexOf('.');
                var extensions = index > 0 ? fileName.Remove(0,index) : fileName;

                if (".html".Equals(extensions)|| ".htm".Equals(extensions)|| ".jsp".Equals(extensions))
                {
                    ctype = "text/html";
                }
                else if (".jpeg".Equals(extensions)|| ".jpg".Equals(extensions)|| ".jpe".Equals(extensions))
                {
                    ctype = "image/jpeg";
                }else if (".png".Equals(extensions))
                {
                    ctype = "image/png";
                }else if (".xml".Equals(extensions))
                {
                    ctype = "text/xml";
                }
                else if (".js".Equals(extensions))
                {
                    ctype = "application/x-javascript";
                }
                else if (".css".Equals(extensions))
                {
                    ctype = "text/css";
                }

                if (string.IsNullOrEmpty(ctype))
                {
                    //todo:读取Mime-Type文件
                    //此处可以读取约定的Mime-Type文件，来确定请求类型
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("ContextType" + exception.Message);
                ctype = "text/html";
            }

            return ctype;
        }

        public static string GetContextTypeByFormat(UriFormat a)
        {
            return null;
        }
    }
}