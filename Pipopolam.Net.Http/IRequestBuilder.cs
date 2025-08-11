using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Pipopolam.Net.Http;

public interface IRequestBuilder<T> where T : IRequestBuilder<T>
{
    /// <summary>
    /// Add segment to the URL path.
    /// <example>
    /// For example, <c>AddSegment("test")</c> changes "https://example.com/service" to "https://example.com/service/test".
    /// </example>
    /// </summary>
    /// <param name="segment">Path segment string to add.</param>
    /// <exception cref="InvalidOperationException">Throws when <paramref name="segment"/> is an empty string.</exception>
    T AddSegment(string segment);

    /// <summary>
    /// Add segmented path to the URL path.
    /// <example>
    /// For example, <c>AddPath("service/test")</c> changes "https://example.com/" to "https://example.com/service/test".
    /// </example>
    /// </summary>
    /// <param name="segments">Path to add. It must be string delimeted by '/'</param>
    T AddPath(string segments);

    /// <summary>
    /// Adds query parameter.
    /// </summary>
    T AddQueryParameter(string key, string? value);

    /// <summary>
    /// Add custom header to the request.
    /// </summary>
    T AddHeader(string key, string? value);

    /// <summary>
    /// Adds body to the request.
    ///
    /// <typeparamref name="TRequest"/> is used to determine how to serialize the <paramref name="body"/> using next logic:
    /// <list type="number">
    /// <item>When <typeparamref name="TRequest"/> is <see cref="Stream"/> binary format will be used.</item>
    /// <item>For <c>IEnumerable<KeyValuePair<string, string>></c> Url Encoded form will be used.</item>
    /// <item>For <c>IEnumerable<KeyValuePair<string, object>></c> Multipart request will be used with the same logic for each element.</item>
    /// <item>For any other case <see cref="WebService.Serializer"/> will be used to serialized <paramref name="body"/>.</item>
    /// </list>
    /// </summary>
    T Body<TRequest>(TRequest? body);
}

public interface IBaseRequestBuilder : IRequestBuilder<IBaseRequestBuilder>
{
    IBaseRequestBuilder SetScheme(UrlScheme scheme);
    IBaseRequestBuilder SetHost(string host);
    IBaseRequestBuilder SetPort(int port);
}

public interface IRequestBuilder : IRequestBuilder<IRequestBuilder>
{
    IRawRequestBuilder UseRawResponse();

    Task Request(HttpMethod method);
    Task Request(HttpMethod method, CancellationToken token);
    Task<TResponse> Request<TResponse>(HttpMethod method);
    Task<TResponse> Request<TResponse>(HttpMethod method, CancellationToken token);
}

public interface IRawRequestBuilder : IRequestBuilder<IRawRequestBuilder>
{
    Task<Response> Request(HttpMethod method);
    Task<Response> Request(HttpMethod method, CancellationToken token);
    Task<Response<TResponse>> Request<TResponse>(HttpMethod method);
    Task<Response<TResponse>> Request<TResponse>(HttpMethod method, CancellationToken token);
}
