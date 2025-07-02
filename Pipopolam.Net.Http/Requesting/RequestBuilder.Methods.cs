using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Pipopolam.Net.Http.Requesting;

partial class RequestBuilder : IRequest
{
    public Task Request(HttpMethod method)
    {
        CancellationTokenSource source = new CancellationTokenSource();
        throw new NotImplementedException();
        // return new Request(Service.Request(method, this, source.Token), source);
    }

    public Task Request(HttpMethod method, CancellationToken token)
    {
        CancellationTokenSource requestSource = new CancellationTokenSource();
        CancellationTokenSource source = CancellationTokenSource.CreateLinkedTokenSource(requestSource.Token, token);
        throw new NotImplementedException();
        // return new Request(Service.Request(method, this, source.Token), requestSource, source);
    }

    public Task<TResponse> Request<TResponse>(HttpMethod method)
    {
        CancellationTokenSource source = new CancellationTokenSource();
        throw new NotImplementedException();
        // return new Request<TResponse>(Service.Request<TResponse>(method, this, source.Token), source);
    }

    public Task<TResponse> Request<TResponse>(HttpMethod method, CancellationToken token)
    {
        CancellationTokenSource requestSource = new CancellationTokenSource();
        CancellationTokenSource source = CancellationTokenSource.CreateLinkedTokenSource(requestSource.Token, token);
        throw new NotImplementedException();
        // return new Request<TResponse>(Service.Request<TResponse>(method, this, source.Token), requestSource, source);
    }

    /// <summary>
    /// Finalizes request and start it using GET method.
    /// </summary>
    /// <typeparam name="TResult">Expected result type.</typeparam>
    /// <returns>Awaitable request with <typeparamref name="TResult"/> result.</returns>
    public Task<TResult> Get<TResult>()
    {
        return Request<TResult>(HttpMethod.Get);
    }

    /// <summary>
    /// Finalizes request and start it using GET method.
    /// </summary>
    /// <typeparam name="TResult">Expected result type.</typeparam>
    /// <returns>Awaitable request with <typeparamref name="TResult"/> result.</returns>
    public Task<TResult> Get<TResult>(CancellationToken token)
    {
        return Request<TResult>(HttpMethod.Get, token);
    }

    /// <summary>
    /// Finalizes request and start it using POST method.
    /// </summary>
    /// <returns>Awaitable request.</returns>
    public Task Post()
    {
        return Request(HttpMethod.Post);
    }

    /// <summary>
    /// Finalizes request and start it using POST method.
    /// </summary>
    /// <returns>Awaitable request.</returns>
    public Task Post(CancellationToken token)
    {
        return Request(HttpMethod.Post, token);
    }

    /// <summary>
    /// Finalizes request and start it using POST method.
    /// </summary>
    /// <typeparam name="TResult">Expected result type.</typeparam>
    /// <returns>Awaitable request with <typeparamref name="TResult"/> result.</returns>
    public Task<TResponse> Post<TResponse>()
    {
        return Request<TResponse>(HttpMethod.Post);
    }

    /// <summary>
    /// Finalizes request and start it using POST method.
    /// </summary>
    /// <typeparam name="TResult">Expected result type.</typeparam>
    /// <returns>Awaitable request with <typeparamref name="TResult"/> result.</returns>
    public Task<TResponse> Post<TResponse>(CancellationToken token)
    {
        return Request<TResponse>(HttpMethod.Post, token);
    }

    /// <summary>
    /// Finalizes request and start it using PUT method.
    /// </summary>
    /// <returns>Awaitable request.</returns>
    public Task Put()
    {
        return Request(HttpMethod.Put);
    }

    /// <summary>
    /// Finalizes request and start it using PUT method.
    /// </summary>
    /// <returns>Awaitable request.</returns>
    public Task Put(CancellationToken token)
    {
        return Request(HttpMethod.Put, token);
    }

    /// <summary>
    /// Finalizes request and start it using PUT method.
    /// </summary>
    /// <typeparam name="TResult">Expected result type.</typeparam>
    /// <returns>Awaitable request with <typeparamref name="TResult"/> result.</returns>
    public Task<TResponse> Put<TResponse>()
    {
        return Request<TResponse>(HttpMethod.Put);
    }

    /// <summary>
    /// Finalizes request and start it using PUT method.
    /// </summary>
    /// <typeparam name="TResult">Expected result type.</typeparam>
    /// <returns>Awaitable request with <typeparamref name="TResult"/> result.</returns>
    public Task<TResponse> Put<TResponse>(CancellationToken token)
    {
        return Request<TResponse>(HttpMethod.Put, token);
    }

    /// <summary>
    /// Finalizes request and start it using DELETE method.
    /// </summary>
    /// <returns>Awaitable request.</returns>
    public Task Delete()
    {
        return Request(HttpMethod.Delete);
    }

    /// <summary>
    /// Finalizes request and start it using DELETE method.
    /// </summary>
    /// <returns>Awaitable request.</returns>
    public Task Delete(CancellationToken token)
    {
        return Request(HttpMethod.Delete, token);
    }

    /// <summary>
    /// Finalizes request and start it using DELETE method.
    /// </summary>
    /// <typeparam name="TResult">Expected result type.</typeparam>
    /// <returns>Awaitable request with <typeparamref name="TResult"/> result.</returns>
    public Task<TResponse> Delete<TResponse>()
    {
        return Request<TResponse>(HttpMethod.Delete);
    }

    /// <summary>
    /// Finalizes request and start it using DELETE method.
    /// </summary>
    /// <typeparam name="TResult">Expected result type.</typeparam>
    /// <returns>Awaitable request with <typeparamref name="TResult"/> result.</returns>
    public Task<TResponse> Delete<TResponse>(CancellationToken token)
    {
        return Request<TResponse>(HttpMethod.Delete, token);
    }
}
