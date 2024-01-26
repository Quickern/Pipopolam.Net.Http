using System;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Diagnostics;
using System.Threading;
using Pipopolam.Net.Http.Serialization;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Pipopolam.Net.Http
{
    public abstract class WebService : IDisposable
    {
        public const double Timeout = 2;

        private readonly bool _critical;
        private CancellationTokenSource _allRequestsTokenSource = new CancellationTokenSource();

        private int requestId = 0;

        protected virtual bool EnableLogging => false;

        protected virtual UrlScheme DefaultProtocol => UrlScheme.Https;

        /// <summary>
        /// Base host for all requests.
        /// </summary>
        public abstract string BaseHost { get; }

        private CookieContainer? _cookies;
        protected CookieContainer Cookies => _cookies ??= new CookieContainer();

        private HttpMessageHandler? _messageHandler;
        private HttpMessageHandler MessageHandler
        {
            get
            {
                if (_messageHandler == null)
                {
                    _messageHandler = CreateHandler();

                    if (_messageHandler is HttpClientHandler clientHandler)
                    {
                        clientHandler.CookieContainer = Cookies;
                        clientHandler.UseCookies = true;
                    }
                }

                return _messageHandler;
            }
        }

        private HttpClient? _client;
        private HttpClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new HttpClient(MessageHandler);
                    if (_critical)
                        _client.Timeout = TimeSpan.FromSeconds(Timeout);
                }

                return _client;
            }
        }

        private ISerializer? _serializer;
        public ISerializer Serializer => _serializer ??= CreateSerializer();

        /// <summary>
        /// Base constructor for any web-service provider.
        /// </summary>
        /// <param name="critical">
        /// For critical services 2 seconds timeout will be used for every request.
        /// Will be removed in the future releases and replaced with some way to set service timeout.
        /// </param>
        protected WebService(bool critical = true)
        {
            _critical = critical;
        }

        /// <summary>
        /// Can be used in Xamarin project to use Native handler. Or for mocking purposes in tests.
        /// </summary>
        /// <returns>Message handler for HttpClient.</returns>
        protected virtual HttpMessageHandler CreateHandler() => new HttpClientHandler();

        /// <summary>
        /// Override this method to change serializer.
        /// </summary>
        /// <returns></returns>
        protected virtual ISerializer CreateSerializer()
        #if NETCOREAPP3_0_OR_GREATER
            => new NetJsonSerializer();
        #else
            => new DataContractSerializer();
        #endif

        /// <summary>
        /// Create base request to extend it.
        /// </summary>
        /// <returns>New request builder.</returns>
        public virtual RequestBuilder CreateRequest()
        {
            RequestBuilder builder = new RequestBuilder(this, DefaultProtocol, BaseHost);
            GenericServicePath(builder);
            return builder;
        }

        /// <summary>
        /// Override this and add service path to url builder.
        /// </summary>
        /// <param name="builder">Url builder</param>
        protected abstract void GenericServicePath(RequestBuilder builder);

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

        public async Task<ServiceResponse> Request(HttpMethod method, RequestBuilder requestInfo, CancellationToken token)
        {
            int id = Interlocked.Increment(ref requestId);

            using CancellationTokenSource tcs = CancellationTokenSource.CreateLinkedTokenSource(_allRequestsTokenSource.Token, token);

            await LogRequest(id, requestInfo);

            HttpResponseMessage resp = await RequestInternal(method, requestInfo, tcs.Token);

            await CheckResponse(await resp.Content.ReadAsStreamAsync(), tcs.Token);

            return new ServiceResponse(resp.Headers);
        }

        public async Task<ServiceResponse<TResponse>> Request<TResponse>(HttpMethod method, RequestBuilder requestInfo, CancellationToken token)
            where TResponse : class
        {
            int id = Interlocked.Increment(ref requestId);

            using CancellationTokenSource tcs = CancellationTokenSource.CreateLinkedTokenSource(_allRequestsTokenSource.Token, token);

            await LogRequest(id, requestInfo);

            HttpResponseMessage resp = await RequestInternal(method, requestInfo, tcs.Token);

            Stream serialized = await resp.Content.ReadAsStreamAsync();
            await LogResponse(id, serialized);

            await CheckResponse(serialized, tcs.Token);

            try
            {
                serialized.Seek(0, SeekOrigin.Begin);
                if (typeof(TResponse) == typeof(string))
                {
                    using (TextReader reader = new StreamReader(serialized))
                    {
                        string response = await reader.ReadToEndAsync();
                        return new ServiceResponse<TResponse>(response as TResponse, resp.Headers);
                    }
                }
                else
                {
                    TResponse? response = await Serializer.DeserializeAsync<TResponse>(serialized, tcs.Token);
                    return new ServiceResponse<TResponse>(response, resp.Headers);
                }
            }
            catch (Exception ex)
            {
                Log($"[Error] Error while parsing response: {ex.Message}");

                throw new WebServiceException($"Can't parse response of type '{typeof(TResponse).Name}': {await ParseError(serialized)}", ex);
            }
        }

        private protected async Task<string?> ParseError(Stream serialized)
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
                Log($"[Error] Error while reading response: {ex.Message}");
                return null;
            }
        }

        private protected virtual Task CheckResponse(Stream serialized, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        [DoesNotReturn]
        private protected virtual async Task HandleRemoteError(HttpStatusCode code, Stream serialized, CancellationToken token)
        {
            throw new WebServiceRemoteException(code, await ParseError(serialized));
        }

        private async Task<HttpResponseMessage> RequestInternal(HttpMethod method, RequestBuilder requestInfo, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            HttpRequestMessage request = new HttpRequestMessage(method, requestInfo.BuildUrl());
            foreach (KeyValuePair<string, string?> kvp in requestInfo.Headers)
            {
                request.Headers.Add(kvp.Key, kvp.Value);
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
            catch (HttpRequestException ex)
            {
                Log($"[Error] Got HttpRequestException: {ex.Message}");

                throw new WebServiceNoConnectionException(ex);
            }
        }

        private async Task LogRequest(int id, RequestBuilder requestInfo)
        {
            if (!EnableLogging)
                return;

            Log($"{BaseHost} Request {id}: {requestInfo.BuildUrl()}");
            if (requestInfo.Content is FormUrlEncodedContent || requestInfo.Content is StringContent)
                Log($"{BaseHost} Request {id} body: {await requestInfo.Content.ReadAsStringAsync()}");
        }

        private async Task LogResponse(int id, Stream serialized)
        {
            if (!EnableLogging)
                return;

            try
            {
                serialized.Seek(0, SeekOrigin.Begin);
                using (TextReader reader = new StreamReader(serialized))
                {
                    string debug = await reader.ReadToEndAsync();
                    Log($"{BaseHost} Request {id} received: {debug}");
                }
            }
            catch
            {
                Log($"{BaseHost} Request {id} can't read response");
            }
        }

        private protected void Log(string str)
        {
            if (!EnableLogging)
                return;

            WriteLog($"[WebService] {str}");
        }

        protected virtual void WriteLog(string logMessage)
        {
            Debug.WriteLine(logMessage);
        }
    }

    public abstract class WebService<TError> : WebService
        where TError: class
    {
        /// <summary>
        /// Can be overridden to try to deserialize errors before response.
        /// Some protocols uses 200 (OK) response with error data instead of correct response.
        /// true for any service with IBasicResponse error type.
        /// </summary>
        protected virtual bool PrehandleErrors => typeof(IBasicResponse).IsAssignableFrom(typeof(TError));

        protected WebService(bool critical = true) : base(critical) { }

        private protected override async Task CheckResponse(Stream serialized, CancellationToken token)
        {
            if (!PrehandleErrors)
                return;

            try
            {
                serialized.Seek(0, SeekOrigin.Begin);

                if (await Serializer.DeserializeAsync<TError>(serialized, token) is TError response &&
                        (response is IBasicResponse basicResponse) && !basicResponse.Success)
                    throw new WebServiceErrorException<TError>(response);
            }
            catch (Exception ex) when (!(ex is WebServiceException))
            {
                Log("[Error] Error while parsing error: " + ex);
                throw new WebServiceException($"Can't parse error of type '{typeof(TError).Name}': {await ParseError(serialized)}", ex);
            }
        }

        [DoesNotReturn]
        private protected override async Task HandleRemoteError(HttpStatusCode code, Stream serialized, CancellationToken token)
        {
            try
            {
                serialized.Seek(0, SeekOrigin.Begin);

                if (await Serializer.DeserializeAsync<TError>(serialized, token) is TError response)
                    throw new WebServiceRemoteException<TError>(code, response);

                await base.HandleRemoteError(code, serialized, token);
            }
            catch (Exception ex) when (!(ex is WebServiceException))
            {
                Log("[Error] Error while parsing remote error: " + ex);

                await base.HandleRemoteError(code, serialized, token);
            }
        }
    }
}
