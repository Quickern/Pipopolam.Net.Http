using System;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Threading;
using Pipopolam.Net.Http.Serialization;
using System.Diagnostics.CodeAnalysis;
using Pipopolam.Net.Http.Logging;
using System.Linq;

namespace Pipopolam.Net.Http;

public class WebService : IDisposable
{
    private CancellationTokenSource _allRequestsTokenSource = new CancellationTokenSource();

    private int _requestId = 0;

    public WebServiceOptions Options { get; }

    public TimeSpan? Timeout { get; }

    private HttpMessageHandler? CustomMessageHandler { get; }

    private HttpClient? _client;
    private HttpClient Client
    {
        get
        {
            if (_client == null)
            {
                _client = CustomMessageHandler == null ? new HttpClient() : new HttpClient(CustomMessageHandler);

                if (Timeout != null)
                    _client.Timeout = Timeout.Value;
            }

            return _client;
        }
    }

    public ISerializer Serializer { get; }
    public IWebServiceLogger Logger { get; }

    /// <summary>
    /// Base constructor for any web-service provider.
    /// </summary>
    /// <param name="critical">
    /// For critical services 2 seconds timeout will be used for every request.
    /// Will be removed in the future releases and replaced with some way to set service timeout.
    /// </param>
    public WebService(WebServiceOptions settings)
    {
        Options = settings;
    }

    /// <summary>
    /// Cancels all current requests. You can send new requests after that.
    /// </summary>
    public void Close()
    {
        CancellationTokenSource tcs = Interlocked.Exchange(ref _allRequestsTokenSource, new CancellationTokenSource());
        tcs.Cancel();
        tcs.Dispose();
    }

    public void Dispose()
    {
        _allRequestsTokenSource.Cancel();
        _client?.Dispose();
        _allRequestsTokenSource.Dispose();
    }

    internal async Task<Response<TError>> Request<TError>(HttpMethod method, Request requestInfo, CancellationToken cancellationToken, CancellationToken timeoutToken)
    {
        int id = Interlocked.Increment(ref _requestId);

        HttpResponseMessage resp = await RequestInternal(method, requestInfo, cancellationToken, timeoutToken).ConfigureAwait(false);

        await IsSuccessResponse(resp.StatusCode, resp.Content, cancellationToken);

        return new Response<TError>(resp.Headers, null);
    }

    internal async Task<Response<TResponse, TError>> Request<TResponse, TError>(HttpMethod method, Request requestInfo, CancellationToken cancellationToken, CancellationToken timeoutToken)
        where TResponse : class
    {
        int id = Interlocked.Increment(ref _requestId);

        HttpResponseMessage resp = await RequestInternal(method, requestInfo, cancellationToken, timeoutToken);

        if (!await IsSuccessResponse(resp.StatusCode, resp.Content, cancellationToken).ConfigureAwait(false))
        {

        }

        try
        {
            TResponse? response = await Serializer.DeserializeAsync<TResponse>(serialized, cancellationToken);
            return new Response<TResponse, TError>(resp.StatusCode, resp.Headers.ToDictionary(x => x.Key, x => x.Value), response);
        }
        catch (Exception ex)
        {
            Logger.LogError(id, $"Error while parsing response", ex);

            throw new WebServiceException($"Can't parse response of type '{typeof(TResponse).Name}': {await ParseError(id, serialized)}", ex);
        }
    }

    private protected async Task<string?> ParseError(int id, HttpContent content)
    {
        try
        {
            serialized.Seek(0, SeekOrigin.Begin);
            using (TextReader reader = new StreamReader(serialized))
            {
                return await reader.ReadToEndAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(id, $"Error while reading response", ex);
            return null;
        }
    }

    private protected virtual Task<bool> IsSuccessResponse(HttpStatusCode code, HttpContent content, CancellationToken token)
    {
        return Task.FromResult(true);
    }

    [DoesNotReturn]
    private protected virtual async Task HandleRemoteError(int id, HttpStatusCode code, Stream serialized, CancellationToken token)
    {
        throw new WebServiceRemoteException<string?>(code, await ParseError(id, serialized));
    }

    private async Task<HttpResponseMessage> RequestInternal(HttpMethod method, Request requestInfo, CancellationToken cancellationToken, CancellationToken timeoutToken)
    {
        token.ThrowIfCancellationRequested();

        HttpRequestMessage request = new HttpRequestMessage(method, requestInfo.Uri);
        foreach ((string name, string value) in requestInfo.Headers)
        {
            request.Headers.Add(name, value);
        }
        if (requestInfo.Content != null)
            request.Content = requestInfo.Content;
        try
        {
            HttpResponseMessage resp = await Client.SendAsync(request, token);
            if (resp.IsSuccessStatusCode)
            {
                return resp;
            }
            else
            {
                Stream serialized = await resp.Content.ReadAsStreamAsync();

                await HandleRemoteError(resp.StatusCode, serialized, token);

                throw new NotSupportedException("Inaccessible code detected");
            }
        }
        catch (TaskCanceledException ex)
        {
            Log($"[Error] Got TaskCanceledException: {ex.Message}");

            if (token.IsCancellationRequested)
                throw;

            throw new WebServiceNoConnectionException(ex);
        }
    }
}
