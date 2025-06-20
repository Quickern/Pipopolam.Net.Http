using System.Net.Http.Headers;

namespace Pipopolam.Net.Http
{
    public class Response
    {
        public HttpResponseHeaders Headers { get; }

        internal Response(HttpResponseHeaders headers)
        {
            Headers = headers;
        }
    }

    public class Response<T> : Response
    {
        public T? Data { get; }

        internal Response(T? data, HttpResponseHeaders headers) : base(headers)
        {
            Data = data;
        }
    }

    /// <summary>
    /// Error for services with error prehandling need to implement <see cref="IBasicResponse" /> interface
    /// </summary>
    public interface IBasicResponse
    {
        bool IsSuccess { get; }
    }
}
