using System.Net;

namespace Common.Tools.HttpServer
{
    /// <summary>
    /// 响应GET请求代理声明
    /// </summary>
    /// <param name="context">http请求上下文</param>
    /// <returns></returns>
    public delegate byte[] DoGet(HttpListenerContext context);

    /// <summary>
    /// 响应POST请求代理声明
    /// </summary>
    /// <param name="context">http请求上下文</param>
    /// <returns></returns>
    public delegate string DoPost(HttpListenerContext context);

    /// <summary>
    /// Http监听服务接口
    /// </summary>
    public interface IHttpServer
    {
        /// <summary>
        /// 响应GET代理
        /// </summary>
        DoGet DoGetMethod { get; set; }

        /// <summary>
        /// 响应POST代理
        /// </summary>
        DoPost DoPostMethod { get; set; }

        /// <summary>
        /// 端口，默认80
        /// </summary>
        short Port { get; set; }
        /// <summary>
        /// 服务名
        /// </summary>
        string ServerName { get; set; }

        bool Start();

        void Stop();
    }
}