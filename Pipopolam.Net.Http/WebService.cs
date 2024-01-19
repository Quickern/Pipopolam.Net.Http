#if DEBUG
#define WEB_SERVICE_LOGS
#endif

using System;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Diagnostics;
using System.Threading;
using Pipopolam.Net.Http.Serialization;

namespace Pipopolam.Net.Http
{
    public abstract class WebService : IDisposable
    {
        public const double Timeout = 2;

        private readonly bool _critical;
        private CancellationTokenSource _allRequestsTokenSource = new CancellationTokenSource();

        private int requestId = 0;

        /// <summary>
        /// Base host for all requests.
        /// </summary>
        public abstract string BaseHost { get; }

        /// <summary>
        /// Override this to try to deserialize error first. Some protocols uses 200 (OK) response with error data instead of correct response.
        /// </summary>
        protected virtual bool PrehandleErrors => false;

        protected virtual UrlScheme DefaultProtocol => UrlScheme.Https;

        private CookieContainer _cookies;
        protected CookieContainer Cookies => _cookies ??= new CookieContainer();

        private HttpMessageHandler _messageHandler;
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

        private HttpClient _client;
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

        private ISerializer _serializer;
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

            Log($"{BaseHost} Request {id}: {requestInfo.BuildUrl()}");
            if (requestInfo.Content is FormUrlEncodedContent || requestInfo.Content is StringContent)
                Log($"{BaseHost} Request {id} body: {await requestInfo.Content.ReadAsStringAsync()}");

            HttpResponseMessage resp = await RequestInternal(method, requestInfo, tcs.Token);

            if (PrehandleErrors)
                await ErrorHandler(await resp.Content.ReadAsStreamAsync(), tcs.Token);

            return new ServiceResponse(resp.Headers);
        }

        public async Task<ServiceResponse<TResponse>> Request<TResponse>(HttpMethod method, RequestBuilder requestInfo, CancellationToken token)
            where TResponse : class
        {
            int id = Interlocked.Increment(ref requestId);

            using CancellationTokenSource tcs = CancellationTokenSource.CreateLinkedTokenSource(_allRequestsTokenSource.Token, token);

            Log($"{BaseHost} Request {id}: {requestInfo.BuildUrl()}");
            if (requestInfo.Content is FormUrlEncodedContent || requestInfo.Content is StringContent)
                Log($"{BaseHost} Request {id} body: {await requestInfo.Content.ReadAsStringAsync()}");

            HttpResponseMessage resp = await RequestInternal(method, requestInfo, tcs.Token);

            Stream serialized = await resp.Content.ReadAsStreamAsync();
            LogResponse(id, serialized);

            if (PrehandleErrors)
                await ErrorHandler(serialized, tcs.Token);

            try
            {
                serialized.Seek(0, SeekOrigin.Begin);
                if (typeof(TResponse) == typeof(string))
                {
                    TextReader reader = new StreamReader(serialized);
                    string response = reader.ReadToEnd();
                    return new ServiceResponse<TResponse>(response as TResponse, resp.Headers);
                }
                else
                {
                    TResponse response = await Serializer.DeserializeAsync<TResponse>(serialized, tcs.Token);
                    return new ServiceResponse<TResponse>(response, resp.Headers);
                }
            }
            catch (Exception ex)
            {
                Log("[Error] Error while parsing response: " + ex);

                await ErrorHandler(serialized, tcs.Token);
                return null;
            }
        }

        private string ParseError(Stream serialized)
        {
            serialized.Seek(0, SeekOrigin.Begin);
            using (TextReader reader = new StreamReader(serialized))
            {
                return reader.ReadToEnd();
            }
        }

        protected virtual Task ErrorHandler(Stream serialized, CancellationToken token)
        {
            throw new WebServiceErrorException(ParseError(serialized));
        }

        protected virtual Task RemoteErrorHandler(HttpStatusCode code, Stream serialized, CancellationToken token)
        {
            throw new WebServiceRemoteException(code, ParseError(serialized));
        }

        private async Task<HttpResponseMessage> RequestInternal(HttpMethod method, RequestBuilder requestInfo, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            HttpRequestMessage request = new HttpRequestMessage(method, requestInfo.BuildUrl());
            foreach (var kvp in requestInfo.Headers)
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

                    await RemoteErrorHandler(resp.StatusCode, serialized, token);

                    throw new NotSupportedException("Inaccessible code detected");
                }
            }
            catch (TaskCanceledException ex)
            {
                Log("[Error] Got TaskCanceledException: " + ex);

                if (token.IsCancellationRequested)
                    throw;

                throw new WebServiceNoConnectionException(ex);
            }
            catch (HttpRequestException ex)
            {
                Log("[Error] Got HttpRequestException: " + ex);

                throw new WebServiceNoConnectionException(ex);
            }
        }

        [Conditional("WEB_SERVICE_LOGS")]
        private void LogResponse(int id, Stream serialized)
        {
            try
            {
                serialized.Seek(0, SeekOrigin.Begin);
                TextReader reader = new StreamReader(serialized);
                string debug = reader.ReadToEnd();
                Log($"{BaseHost} Request {id} received: {debug}");
            }
            catch
            {
                Log($"{BaseHost} Request {id} can't read response");
            }
        }

        [Conditional("WEB_SERVICE_LOGS")]
        protected void Log(string str)
        {
            Debug.WriteLine("[WebService] " + str);
        }
    }

    public abstract class WebService<TDefaultError> : WebService
        where TDefaultError: class
    {
        protected override bool PrehandleErrors => typeof(IBasicResponse).IsAssignableFrom(typeof(TDefaultError));

        protected WebService(bool critical = true) : base(critical) { }

        protected override async Task ErrorHandler(Stream serialized, CancellationToken token)
        {
            try
            {
                serialized.Seek(0, SeekOrigin.Begin);

                if (await Serializer.DeserializeAsync<TDefaultError>(serialized, token) is TDefaultError response &&
                        (response is IBasicResponse basicResponse) && !basicResponse.Success)
                    throw new WebServiceErrorException<TDefaultError>(response);
            }
            catch (Exception ex) when (!(ex is WebServiceErrorException<TDefaultError>))
            {
                Log("[Error] while parsing error: " + ex);

                await base.ErrorHandler(serialized, token);
            }
        }

        protected override async Task RemoteErrorHandler(HttpStatusCode code, Stream serialized, CancellationToken token)
        {
            try
            {
                serialized.Seek(0, SeekOrigin.Begin);

                if (await Serializer.DeserializeAsync<TDefaultError>(serialized, token) is TDefaultError response)
                    throw new WebServiceRemoteException<TDefaultError>(code, response);
            }
            catch (Exception ex) when (!(ex is WebServiceRemoteException<TDefaultError>))
            {
                Log("[Error] while parsing error: " + ex);

                await base.ErrorHandler(serialized, token);
            }
        }
    }
}
