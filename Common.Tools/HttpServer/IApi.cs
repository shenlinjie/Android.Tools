using System.Net;

namespace Common.Tools.HttpServer
{
    public delegate string ApiAction(HttpListenerContext context, string actionName);
    public interface IApi
    {
        /// <summary>
        /// Api运行状态，只有运行状态下才响应api请求，需要手动设置
        /// </summary>
        bool WorkingState { get; set; }
        /// <summary>
        /// 操作码，即继承类中的方法备注[使用ApiPath备注]
        /// </summary>
        string ActionString { get; set; }
        /// <summary>
        /// api响应请求代理，
        /// </summary>
        ApiAction Action { get; set; }
        /// <summary>
        /// http请求上下文
        /// </summary>
        HttpListenerContext Content { get; set; }
        /// <summary>
        /// 处理结果标识
        /// </summary>
        bool Success { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        string Error { get; set; }

    }
}
