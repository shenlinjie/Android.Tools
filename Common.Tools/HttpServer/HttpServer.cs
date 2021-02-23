using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using Common.Tools.Attribute;

namespace Common.Tools.HttpServer
{
    /// <inheritdoc />
    /// <summary>
    /// 
    /// </summary>
    public abstract class HttpServer:IHttpServer
    {
        protected internal string Nullstring = string.Empty;
        /// <summary>
        /// http监听，负责对外提供服务
        /// </summary>
        internal HttpListener Listener { get; set; }

        /// <summary>
        /// 响应主线程
        /// </summary>
        private Thread MainThread { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// GET HttpHandler
        /// </summary>
        public DoGet DoGetMethod { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// POST HttpHandler
        /// </summary>
        public DoPost DoPostMethod { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// 服务发布端口
        /// </summary>
        public short Port { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// 服务名
        /// 如 http://ip:port/sn 中sn的值
        /// </summary>
        public string ServerName { get; set; }

        protected string UriFormat = "http://*:{0}/";

        protected internal string Ots = "out of Service";

        protected internal IApi Api { get; set; }

        public List<IApi> Apis { get; set; }

        public Assembly Asm { get; set; }

        private bool _isRunning = true;

        /// <summary>
        /// 指南针自定义网络服务
        /// </summary>
        /// <param name="sn">服务名</param>
        /// <param name="port">端口号</param>
        /// <param name="asm"></param>
        protected HttpServer(string sn,short port=80)
        {
            Port = port;
            ServerName = sn;
            Init();
            DoGetMethod = content =>
            {
                var fileName = content.Request.Url.AbsolutePath.Remove(0, 1);
#if DEBUG
                Console.WriteLine(fileName);
#endif
                fileName = !File.Exists(fileName)?AnalysisFile(fileName): fileName;
                if (File.Exists(fileName))
                {
                    var reader = new FileStream(fileName, FileMode.Open);
                    var bytes = new byte[reader.Length];

                    reader.Read(bytes, 0, bytes.Length);
                    reader.Close();
                    return bytes;
                }

                var postResult = DoPostMethod.Invoke(content);
                return Encoding.UTF8.GetBytes(string.IsNullOrEmpty(postResult) ? Nullstring : postResult);
            };

            DoPostMethod = content =>
            {
                string result;
                //获取访问路径
                var path = content.Request.Url.AbsolutePath.Remove(0, 1);

                string[] strs = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                if (strs.Length <= 2 || !sn.Equals(strs[0]))
                {
                    return Nullstring;
                }
                //if (!sn.Equals(strs[0])) return Nullstring;
                var service = strs[1];
                var action = strs[2];
                //判断是否调用的是本模块
                if (Api != null)
                {
                    var attrs = Api.GetType().GetCustomAttributes(typeof(ApiPath), true) as ApiPath[];
                    if (attrs == null || !attrs.Any(o => o.Name.Equals(service,StringComparison.CurrentCultureIgnoreCase)))
                    {
                        result = WorkWithConstomApi(service,action, content);
                    }
                    else
                    {
                        result = Api.Action(content,action);
                    }
                }
                else if (Apis != null)
                {
                    result = WorkWithConstomApi(service,action, content);
                }
                else
                {
                    result = Ots;
                }
                return result;
            };
        }

        private string AnalysisFile(string fileName)
        {
            string res;
            if (fileName.EndsWith(".htm") || fileName.EndsWith(".html")) return fileName;
            if (!File.Exists(fileName + ".htm")) res = fileName + ".html";
            else res = fileName + ".htm";

            if (!File.Exists(fileName))
            {
                if (File.Exists(fileName + "/index.htm")) res = fileName + "/index.htm";
                else res = fileName + "/index.html";
            }

            return res;
        }

        private void Init()
        {
            ConfigurationInfo();
            var prefix = string.Format(UriFormat, Port);//string.IsNullOrEmpty(ServerName) ? $"http://*:{Port}/{ServerName}/" : $"http://*:{Port}/";
            if (!string.IsNullOrEmpty(ServerName))
            {
                prefix += ServerName + "/";
            }
            //初始化Linstener
            Listener = new HttpListener()
            {
                AuthenticationSchemes = AuthenticationSchemes.Anonymous,
                Prefixes = {prefix,},
                UnsafeConnectionNtlmAuthentication = true,
                //Realm
            };

            MainThread = new Thread(() =>
            {
                while (_isRunning)
                {
                    if (!Listener.IsListening)continue;
                    //获取请求上下文
                    HttpListenerContext context = null;
                    try
                    {
                        context = Listener.GetContext();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                    if(context == null)continue;
                    var method = context.Request.HttpMethod;
#if DEBUG
                    foreach (var key in context.Request.Headers.AllKeys)
                    {
                        Console.WriteLine($"{key}:{context.Request.Headers[key]}");
                    }
#endif

                    //根据请求方式，处理请求并返回响应信息
                    if ("GET".Equals(method))
                    {
                        if (DoGetMethod == null) return;
                        ThreadPool.QueueUserWorkItem((state =>
                        {
                            var con = state as HttpListenerContext;
                            if (con == null) return;
                            var res = DoGetMethod.Invoke(con);
                            PutGetResponse(res, con);
                        }), context);
                    }
                    else if ("POST".Equals(method))
                    {
                        if (DoPostMethod == null) return;
                        ThreadPool.QueueUserWorkItem(state =>
                        {
                            var con = state as HttpListenerContext;
                            if (con == null) return;
                            var res = DoPostMethod.Invoke(con);
                            PutPostResponse(res, con);
                        }, context);
                    }
                    else
                    {
                        var res = context.Response;
                        res.AppendHeader("Access-Control-Allow-Origin", "*");
                        res.AppendHeader("Access-Control-Allow-Methods", "POST");
                        res.AppendHeader("Access-Control-Allow-Header", "Cookie");
                        res.StatusCode = 200;
                        res.Close();
                    }
                }
            })
            {
                IsBackground = true,Name = $"Http服务主线程-port:{Port}"
            };
        }

        private void ConfigurationInfo()
        {
            if(Asm ==null) Asm = Assembly.GetEntryAssembly(); //如果是当前程序集
            //Console.WriteLine(Asm.FullName);
            var title = ((AssemblyTitleAttribute)Asm.GetCustomAttribute(typeof(AssemblyTitleAttribute)))?.Title;
            var description = ((AssemblyDescriptionAttribute)Asm.GetCustomAttribute(typeof(AssemblyDescriptionAttribute)))?.Description; 
            var copyright = ((AssemblyCopyrightAttribute)Asm.GetCustomAttribute(typeof(AssemblyCopyrightAttribute)))?.Copyright;
            var company = ((AssemblyCompanyAttribute)Asm.GetCustomAttribute(typeof(AssemblyCompanyAttribute)))?.Company;
            var product = ((AssemblyProductAttribute) Asm.GetCustomAttribute(typeof(AssemblyProductAttribute)))
                ?.Product;
            Nullstring =
                $"<html xmlns=\"http://www.w3.org/1999/xhtml\"> <head>" +
                $"<meta http-equiv=\"Content-Type\" content=\"text/html; charset = utf-8\">" +
                $" <title>{title}</title>" +
                $" </head>" +
                $"<body>" +
                $"产品:{product}</br>" +
                $"说明:{description}</br>" +
                $"控制台:Console 404 </br>" +
                $"by {company} </br>" +
                $"{copyright}" +
                $"</body>";
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <returns>是否成功</returns>
        public bool Start()
        {
            try
            {
                _isRunning = true;
                if (Listener != null && !Listener.IsListening)
                    Listener.Start();
                if (!MainThread.IsAlive)
                    MainThread.Start();
                return true;
            }
            catch (HttpListenerException exception)
            {
                var username = Environment.GetEnvironmentVariable("USERNAME");
                var userdomain = Environment.GetEnvironmentVariable("USERDOMAIN");
                if (exception.ErrorCode == 5)
                {
                    var cmd = $"Dear {username}\r\n  You may need to run the following command first:\r\n netsh http add urlacl url=http://*:{Port}/ user={userdomain}\\{username} listen=yes";
                    Console.WriteLine(cmd);
                    Logger.Logger.Error($"权限错误:\r\n{cmd}");
                }
            }
            catch (ThreadStateException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (OutOfMemoryException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        public void Stop()
        {
            try
            {
                //Listener.Stop();
                //MainThread.Abort();
                _isRunning = false;
                Listener.Stop();
                Listener.Close();
                //Container?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }

        public List<ApiPath> GetApiPaths()
        {
            var res = new List<ApiPath> {GetApiPath(Api)};
            if (Apis != null) res.AddRange(Apis.Select(GetApiPath));

            return res;
        }

        private ApiPath GetApiPath(IApi api)
        {
            var temp = api.GetType().GetCustomAttribute(typeof(ApiPath), true) as ApiPath;
            if (temp != null)
            {
                var methods = api.GetType().GetMethods();
                temp.ApiPaths = new List<ApiPath>();
                foreach (var info in methods)
                {
                    if (info.GetCustomAttribute(typeof(ApiPath), true) is ApiPath arr)
                        temp.ApiPaths.Add(arr);
                }
            }

            return temp;
        }


        /// <summary>
        /// 响应GET请求后返回
        /// </summary>
        /// <param name="result"></param>
        /// <param name="con"></param>
        private void PutGetResponse(byte[] result, HttpListenerContext con)
        {
            try
            {
                con.Response.AddHeader("Access-Control-Allow-Origin", "*");
                con.Response.ContentEncoding = Encoding.UTF8;
                con.Response.StatusCode = 200;
                con.Response.ContentType = "text/html";
                con.Response.OutputStream.Write(result, 0, result.Length);
                con.Response.OutputStream.Close();
                con.Response.Close();
            }
            catch (Exception exception)
            {
                Console.WriteLine("HttpServer_PutGetResponse" + exception.Message);
            }
        }

        /// <summary>
        /// 响应POST请求后返回
        /// </summary>
        /// <param name="result"></param>
        /// <param name="con"></param>
        private void PutPostResponse(string result, HttpListenerContext con)
        {
            try
            {
                var data = Encoding.Default.GetBytes(result);
                con.Response.AddHeader("Access-Control-Allow-Origin", "*");
                con.Response.ContentEncoding = Encoding.Default;
                con.Response.StatusCode = 200;
                con.Response.ContentType = ContextType.GetContextTypeByUri(con.Request.Url);
                con.Response.OutputStream.Write(data, 0, data.Length);
                
                con.Response.OutputStream.Close();

                con.Response.Close();
            }
            catch (Exception exception)
            {
                Console.WriteLine("HttpServer_PutPostResponse" + exception.Message);
            }
        }

        private string WorkWithConstomApi(string serivce,string action, HttpListenerContext content)
        {
            string result = string.Empty;
            foreach (var api in Apis)
            {
                var type = api.GetType();
#if  DEBUG
                Console.WriteLine(type.FullName);
#endif
                if (!(type.GetCustomAttributes(typeof(ApiPath), true) is ApiPath[] attrs) || !attrs.Any(o => o.Name.Equals(serivce,StringComparison.CurrentCultureIgnoreCase))) result = Ots;
                else
                {
                    result = api.Action(content, action);
                }
            }
            return result;
        }
    }
}