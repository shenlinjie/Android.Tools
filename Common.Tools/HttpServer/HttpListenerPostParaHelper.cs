using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Common.Tools.HttpServer
{
    /// <summary>
    /// 处理http请求参数助手
    /// </summary>
    public class HttpListenerPostParaHelper
    {
        /// <summary>
        /// 请求上下文
        /// </summary>
        private readonly HttpListenerContext _request;

        public HttpListenerPostParaHelper(HttpListenerContext request)
        {
            this._request = request;
        }

        /// <summary>
        /// 字节数组比较工具
        /// </summary>
        /// <param name="source">源数组（被比较数组）</param>
        /// <param name="comparison">比较数组</param>
        /// <returns></returns>
        private bool CompareBytes(byte[] source, byte[] comparison)
        {
            try
            {
                int count = source.Length;
                if (source.Length != comparison.Length)
                    return false;
                for (int i = 0; i < count; i++)
                    if (source[i] != comparison[i])
                        return false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 流转字节数组
        /// </summary>
        /// <param name="sourceStream"></param>
        /// <returns></returns>
        private byte[] ReadLineAsBytes(Stream sourceStream)
        {
            var resultStream = new MemoryStream();
            while (true)
            {
                int data = sourceStream.ReadByte();
                resultStream.WriteByte((byte)data);
                if (data == 10||data==-1)
                    break;
            }
            resultStream.Position = 0;
            byte[] dataBytes = new byte[resultStream.Length];
            resultStream.Read(dataBytes, 0, dataBytes.Length);
            return dataBytes;
        }

        /// <summary>  
        /// 获取Post过来的参数和数据  
        /// </summary>  
        /// <returns></returns>  
        public List<HttpListenerPostValue> GetHttpListenerPostValue()
        {
            var httpListenerPostValueList = new List<HttpListenerPostValue>();
            try
            {
                var input = _request.Request.InputStream;
                var sr = new StreamReader(input);
                var paramString = sr.ReadToEnd();

                var ps = paramString.Split('&');
                foreach (var s in ps)
                {
                    var kv = s.Split('=');
                    if(kv.Length!=2)continue;
                    httpListenerPostValueList.Add(new HttpListenerPostValue()
                    {
                        Type = 0,Datas = Encoding.UTF8.GetBytes(kv[1]),Name = kv[0]
                    });
                }
                return httpListenerPostValueList;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }
    }
}