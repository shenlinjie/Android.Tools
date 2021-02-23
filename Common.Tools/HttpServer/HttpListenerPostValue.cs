namespace Common.Tools.HttpServer
{
    public class HttpListenerPostValue
    {
        /// <summary>  
        /// 0=> 参数  
        /// 1=> 文件  
        /// </summary>  
        public int Type = 0;
        /// <summary>
        /// 参数名
        /// </summary>
        public string Name;
        /// <summary>
        /// 参数值
        /// </summary>
        public byte[] Datas;
    }
}