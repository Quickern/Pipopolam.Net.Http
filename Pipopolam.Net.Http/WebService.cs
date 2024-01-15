#if DEBUG
#define WEB_SERVICE_LOGS
#endif

using System;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.Threading;
using Pipopolam.Net.Http.Serialization;

namespace Pipopolam.Net.Http
{
    public abstract class WebService
    {
        public const double Timeout = 2;

        private readonly bool _critical;

        private int requestId = 0;

        public abstract string BaseHost { get; }
        protected virtual bool PrehandleErrors => false;
        protected virtual UrlScheme DefaultProtocol => UrlScheme.Https;

        private CookieContainer _cookies;
        protected CookieContainer Cookies => _cookies ??= new CookieContainer();

        private HttpClientHandler _clientHandler;
        private HttpClientHandler ClientHandler
        {
            get
            {
                if (_clientHandler == null)
                {
                    _clientHandler = CreateHandler();

                    _clientHandler.CookieContainer = Cookies;
                    _clientHandler.UseCookies = true;
                }

                return _clientHandler;
            }
        }

        private HttpClient _client;
        private HttpClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new HttpClient(ClientHandler);
                    if (_critical)
                        _client.Timeout = TimeSpan.FromSeconds(Timeout);
                }

                return _client;
            }
        }

        private ISerializer _serializer;
        public ISerializer Serializer => _serializer ??= CreateSerializer();

        protected WebService(bool critical = true)
        {
            _critical = critical;
        }

        protected virtual HttpClientHandler CreateHandler() => new HttpClientHandler();
        protected virtual ISerializer CreateSerializer() => new DataContractSerializer();

        /// <summary>
        /// Create base request to extend it.
        /// </summary>
        /// <returns>RequestInfoBuilder</returns>
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

        public async Task<ServiceResponse> Request(HttpMethod method, RequestBuilder requestInfo, CancellationToken token)
        {
            int id = Interlocked.Increment(ref requestId);
            Log($"{BaseHost} Request {id}: {requestInfo.BuildUrl()}");
            if (method == HttpMethod.Post && requestInfo.Content is FormUrlEncodedContent || requestInfo.Content is StringContent)
            { 
                Log($"{BaseHost} Request {id} body: {await requestInfo.Content.ReadAsStringAsync()}");
            }

            HttpResponseMessage resp = await RequestInternal(method, requestInfo, token);

            if (PrehandleErrors)
                ErrorHandler(await resp.Content.ReadAsStreamAsync());

            return new ServiceResponse(resp.Headers);
        }

        public async Task<ServiceResponse<TResponse>> Request<TResponse>(HttpMethod method, RequestBuilder requestInfo, CancellationToken token)
            where TResponse : class
        {
            int id = Interlocked.Increment(ref requestId);
            Log($"{BaseHost} Request {id}: {requestInfo.BuildUrl()}");
            if (method == HttpMethod.Post && requestInfo.Content != null)
            {
                if (requestInfo.Content is FormUrlEncodedContent || requestInfo.Content is StringContent)
                {
                    string body = await requestInfo.Content.ReadAsStringAsync();
                    Log($"{BaseHost} Request {id} body: {body}");
                }
            }

            HttpResponseMessage resp = await RequestInternal(method, requestInfo, token);

            Stream serialized = await resp.Content.ReadAsStreamAsync();

            LogResponse(id, serialized);

            if (PrehandleErrors)
                ErrorHandler(serialized);

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
                    TResponse response = Serializer.Deserialize<TResponse>(serialized);
                    return new ServiceResponse<TResponse>(response, resp.Headers);
                }
            }
            catch (Exception ex)
            {
                Log("[Error] Error while parsing response: " + ex);

                ErrorHandler(serialized);
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

        protected virtual void ErrorHandler(Stream serialized)
        {
            throw new WebServiceErrorException(ParseError(serialized));
        }

        protected virtual void RemoteErrorHandler(HttpStatusCode code, Stream serialized)
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

                    RemoteErrorHandler(resp.StatusCode, serialized);

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

    public interface IBasicResponse
    {
        bool Success { get; }
    }

    public abstract class WebService<TDefaultError> : WebService
        where TDefaultError: class
    {
        protected override bool PrehandleErrors => typeof(TDefaultError).IsAssignableFrom(typeof(IBasicResponse));

        protected WebService(bool critical = true) : base(critical) { }

        protected override void ErrorHandler(Stream serialized)
        {
            try
            {
                serialized.Seek(0, SeekOrigin.Begin);

                if (Serializer.Deserialize<TDefaultError>(serialized) is TDefaultError response &&
                        (response is IBasicResponse basicResponse) && !basicResponse.Success)
                    throw new WebServiceErrorException<TDefaultError>(response);
            }
            catch (Exception ex) when (!(ex is WebServiceErrorException<TDefaultError>))
            {
                Log("[Error] while parsing error: " + ex);

                base.ErrorHandler(serialized);
            }
        }

        protected override void RemoteErrorHandler(HttpStatusCode code, Stream serialized)
        {
            try
            {
                serialized.Seek(0, SeekOrigin.Begin);

                if (Serializer.Deserialize<TDefaultError>(serialized) is TDefaultError response)
                    throw new WebServiceRemoteException<TDefaultError>(code, response);
            }
            catch (Exception ex) when (!(ex is WebServiceRemoteException<TDefaultError>))
            {
                Log("[Error] while parsing error: " + ex);

                base.ErrorHandler(serialized);
            }
        }
    }

    public class ServiceResponse
    {
        public HttpResponseHeaders Headers { get; private set; }

        public ServiceResponse(HttpResponseHeaders headers)
        {
            Headers = headers;
        }
    }

    public class ServiceResponse<T> : ServiceResponse
    {
        public T Data { get; private set; }

        public ServiceResponse(T data, HttpResponseHeaders headers) : base(headers)
        {
            Data = data;
        }
    }

    public class WebServiceException : Exception
    {
        public WebServiceException() { }

        public WebServiceException(string message) : base(message) { }

        public WebServiceException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class WebServiceNoConnectionException : WebServiceException
    {
        public WebServiceNoConnectionException(Exception innerException) : base("Can't connect to service", innerException) { }
    }

    public class WebServiceErrorException : WebServiceException
    {
        public string Response { get; private set; }

        public WebServiceErrorException(string response)
        {
            Response = response;
        }
    }

    public class WebServiceErrorException<T> : WebServiceException
        where T: class
    {
        public T Response { get; private set; }

        public WebServiceErrorException(T response)
        {
            Response = response;
        }
    }

    public class WebServiceRemoteException : WebServiceErrorException
    {
        public HttpStatusCode StatusCode { get; private set; }

        public WebServiceRemoteException(HttpStatusCode statusCode, string response) :
            base(response)
        {
            StatusCode = statusCode;
        }
    }

    public class WebServiceRemoteException<T> : WebServiceErrorException<T> where T: class
    {
        public HttpStatusCode StatusCode { get; private set; }

        public WebServiceRemoteException(HttpStatusCode statusCode, T response) :
            base(response)
        {
            StatusCode = statusCode;
        }
    }
}
