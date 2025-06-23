using System.Net.Http;
using System.Threading;

namespace Pipopolam.Net.Http.Requesting;

partial class RequestBuilder
{
    /// <summary>
    /// Finalizes request and start it using GET method.
    /// </summary>
    /// <typeparam name="TResult">Expected result type.</typeparam>
    /// <returns>Awaitable request with <typeparamref name="TResult"/> result.</returns>
    public Request<TResult> Get<TResult>() where TResult : class
    {
        return Request<TResult>(HttpMethod.Get);
    }

    /// <summary>
    /// Finalizes request and start it using GET method.
    /// </summary>
    /// <typeparam name="TResult">Expected result type.</typeparam>
    /// <returns>Awaitable request with <typeparamref name="TResult"/> result.</returns>
    public Request<TResult> Get<TResult>(CancellationToken token) where TResult : class
    {
        return Request<TResult>(HttpMethod.Get, token);
    }

    /// <summary>
    /// Finalizes request and start it using POST method.
    /// </summary>
    /// <returns>Awaitable request.</returns>
    public Request Post()
    {
        return Request(HttpMethod.Post);
    }

    /// <summary>
    /// Finalizes request and start it using POST method.
    /// </summary>
    /// <returns>Awaitable request.</returns>
    public Request Post(CancellationToken token)
    {
        return Request(HttpMethod.Post, token);
    }

    /// <summary>
    /// Finalizes request and start it using POST method.
    /// </summary>
    /// <typeparam name="TResult">Expected result type.</typeparam>
    /// <returns>Awaitable request with <typeparamref name="TResult"/> result.</returns>
    public Request<TResponse> Post<TResponse>() where TResponse : class
    {
        return Request<TResponse>(HttpMethod.Post);
    }

    /// <summary>
    /// Finalizes request and start it using POST method.
    /// </summary>
    /// <typeparam name="TResult">Expected result type.</typeparam>
    /// <returns>Awaitable request with <typeparamref name="TResult"/> result.</returns>
    public Request<TResponse> Post<TResponse>(CancellationToken token) where TResponse : class
    {
        return Request<TResponse>(HttpMethod.Post, token);
    }

    /// <summary>
    /// Finalizes request and start it using PUT method.
    /// </summary>
    /// <returns>Awaitable request.</returns>
    public Request Put()
    {
        return Request(HttpMethod.Put);
    }

    /// <summary>
    /// Finalizes request and start it using PUT method.
    /// </summary>
    /// <returns>Awaitable request.</returns>
    public Request Put(CancellationToken token)
    {
        return Request(HttpMethod.Put, token);
    }

    /// <summary>
    /// Finalizes request and start it using PUT method.
    /// </summary>
    /// <typeparam name="TResult">Expected result type.</typeparam>
    /// <returns>Awaitable request with <typeparamref name="TResult"/> result.</returns>
    public Request<TResponse> Put<TResponse>() where TResponse : class
    {
        return Request<TResponse>(HttpMethod.Put);
    }

    /// <summary>
    /// Finalizes request and start it using PUT method.
    /// </summary>
    /// <typeparam name="TResult">Expected result type.</typeparam>
    /// <returns>Awaitable request with <typeparamref name="TResult"/> result.</returns>
    public Request<TResponse> Put<TResponse>(CancellationToken token) where TResponse : class
    {
        return Request<TResponse>(HttpMethod.Put, token);
    }

    /// <summary>
    /// Finalizes request and start it using DELETE method.
    /// </summary>
    /// <returns>Awaitable request.</returns>
    public Request Delete()
    {
        return Request(HttpMethod.Delete);
    }

    /// <summary>
    /// Finalizes request and start it using DELETE method.
    /// </summary>
    /// <returns>Awaitable request.</returns>
    public Request Delete(CancellationToken token)
    {
        return Request(HttpMethod.Delete, token);
    }

    /// <summary>
    /// Finalizes request and start it using DELETE method.
    /// </summary>
    /// <typeparam name="TResult">Expected result type.</typeparam>
    /// <returns>Awaitable request with <typeparamref name="TResult"/> result.</returns>
    public Request<TResponse> Delete<TResponse>() where TResponse : class
    {
        return Request<TResponse>(HttpMethod.Delete);
    }

    /// <summary>
    /// Finalizes request and start it using DELETE method.
    /// </summary>
    /// <typeparam name="TResult">Expected result type.</typeparam>
    /// <returns>Awaitable request with <typeparamref name="TResult"/> result.</returns>
    public Request<TResponse> Delete<TResponse>(CancellationToken token) where TResponse : class
    {
        return Request<TResponse>(HttpMethod.Delete, token);
    }
}
