using System.Net.Http.Headers;

namespace Pipopolam.Net.Http
{
    public class ServiceResponse
    {
        public HttpResponseHeaders Headers { get; private set; }

        public ServiceResponse(HttpResponseHeaders headers)
        {
            Headers = headers;
        }
    }

    public class ServiceResponse<T> : ServiceResponse where T : class
    {
        public T? Data { get; private set; }

        public ServiceResponse(T? data, HttpResponseHeaders headers) : base(headers)
        {
            Data = data;
        }
    }

    /// <summary>
    /// Error for services with error prehandling need to implement <see cref="IBasicResponse" /> interface
    /// </summary>
    public interface IBasicResponse
    {
        bool Success { get; }
    }
}
