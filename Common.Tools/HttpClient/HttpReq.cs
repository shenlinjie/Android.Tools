namespace Common.Tools.HttpClient
{
    public class HttpReq<T>
    {
        public string Guid { get; set; }

        public string Pwd { get; set; }

        public T Data { get; set; }
    }
}