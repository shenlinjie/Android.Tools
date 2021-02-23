namespace Common.Tools.HttpClient
{
    public class HttpRes<T>
    {
        public bool Success { get; set; }

        public string Msg { get; set; }

        public T Data { get; set; }
    }
}