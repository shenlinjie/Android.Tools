using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Common.Tools.Attribute;

namespace Common.Tools.HttpServer
{
    /// <summary>
    /// Api接口的抽象实现，
    /// 包含一些对外api的公用方法的实现
    /// </summary>
    public abstract class Api:IApi
    {
        public bool WorkingState { get; set; }
        public string ActionString { get; set; }
        public ApiAction Action { get; set; }
        public HttpListenerContext Content { get; set; }

        public string ResponseString { get; set; }
        public List<HttpListenerPostValue> Values { get; set; }

        public bool Success { get; set; }

        public string Error { get; set; }

        internal object Waiting = new object();

        internal bool ShowConsole = false;
        protected Api()
        {
            Action = ApiAction;
        }

        private string ApiAction(HttpListenerContext context, string actionName)
        {
            lock (Waiting)
            {
                Content = context;
                ActionString = actionName;
                try
                {
                    var type = GetType();
                    var ms = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var methodInfo in ms)
                    {
                        var mlist = methodInfo.GetCustomAttributes(typeof(ApiPath), true);
                        if (mlist.Length < 1) continue;
                        if (mlist[0] is ApiPath apiPath && mlist.Any() && apiPath.Name.Equals(actionName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            var reqStr = GetInputString(context);
                            var keys = context.Request.QueryString.AllKeys;
                            var pl = methodInfo.GetParameters();
                            if (keys.Length > 0 && pl.Length > 0)
                            {
                                var param = new object[pl.Length];
                                for (int i = 0; i < pl.Length; i++)
                                {
                                    if (keys.Contains(pl[i].Name))
                                    {
                                        param[i] = context.Request.QueryString[pl[i].Name];
                                    }
                                    else
                                    {
                                        param[i] = pl[i].DefaultValue;
                                    }
                                }
                            }

                            var res = methodInfo.Invoke(this, string.IsNullOrEmpty(reqStr) ? null : new[] { reqStr }) as string;
#if DEBUG
                            Console.WriteLine($"Run{methodInfo.Name} param:{reqStr} return:{res}");
#endif
                            Logger.Logger.Debug($"{methodInfo.Name}\r\n param:{reqStr}\r\n return:{res}");
                            return res ?? string.Empty;
                        }
                    }
                }
                catch (Exception exception)
                {
                    Success = false;
#if ShowConsole
                    Console.WriteLine("Api" + exception.Message);
#endif
                    Logger.Logger.Error("API Error", exception);
                }
                return string.Empty;
            }
        }

       
        private string GetInputString(HttpListenerContext context)
        {
            var sr = context.Request.InputStream;
            var bytes = new byte[context.Request.ContentLength64];
            sr.Read(bytes, 0, bytes.Length);
            var res = Encoding.Default.GetString(bytes);
            sr.Close();
            return res;
        }
    }
}