using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace Pipopolam.Net.Http
{
    public class RequestBuilder
    {
        public UrlScheme Scheme { get; private set; } = UrlScheme.Https;
        public string Host { get; private set; }
        public int? Port { get; private set; } = null;
        public IEnumerable<string> Segments { get; private set; } = new List<string>();
        public IEnumerable<QueryParameter> QueryParameters { get; private set; } = new List<QueryParameter>();
        public IDictionary<string, string> Headers { get; private set; } = new Dictionary<string, string>();
        public HttpContent Content { get; private set; }

        public WebService Service { get; private set; }

        public RequestBuilder(WebService service)
        {
            Service = service;
        }

        public RequestBuilder(WebService service, UrlScheme scheme, string host) : this(service)
        {
            SetScheme(scheme);
            SetHost(host);
        }

        public RequestBuilder(WebService service, UrlScheme scheme, string host, int port) : this(service, scheme, host)
        {
            Port = port;
        }

        public RequestBuilder SetScheme(UrlScheme scheme)
        {
            Scheme = scheme;
            return this;
        }

        public RequestBuilder SetHost(string host)
        {
            Host = host;
            return this;
        }

        public RequestBuilder SetPort(int port)
        {
            Port = port;
            return this;
        }

        /// <summary>
        /// Add segment to the URL path.
        /// <example>
        /// For example, <c>AddSegment("test")</c> changes "https://example.com/service" to "https://example.com/service/test".
        /// </example>
        /// </summary>
        /// <param name="segment">Path segment string to add.</param>
        /// <exception cref="InvalidOperationException">Throws when <paramref name="segment"/> is an empty string.</exception>
        public RequestBuilder AddSegment(string segment)
        {
            if (String.IsNullOrWhiteSpace(segment))
                throw new InvalidOperationException("Empty url segment");
            (Segments as IList<string>).Add(segment);
            return this;
        }

        /// <summary>
        /// Add segmented path to the URL path.
        /// <example>
        /// For example, <c>AddPath("service/test")</c> changes "https://example.com/" to "https://example.com/service/test".
        /// </example>
        /// </summary>
        /// <param name="segments">Path to add. It must be string delimeted by '/'</param>
        public RequestBuilder AddPath(string segments)
        {
            string[] s = segments.Split('/');
            foreach (string segment in s)
                AddSegment(segment);
            return this;
        }

        /// <summary>
        /// Adds query parameter.
        /// </summary>
        public RequestBuilder AddQueryParameter(string key, string value)
        {
            (QueryParameters as IList<QueryParameter>).Add(new QueryParameter(key, value));
            return this;
        }

        /// <summary>
        /// Add custom header to the request.
        /// </summary>
        public RequestBuilder AddHeader(string key, string value)
        {
            Headers[key] = value;
            return this;
        }

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
        public RequestBuilder Body<TRequest>(TRequest body) where TRequest : class
        {
            Content = CreateContent(body);
            return this;
        }

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
        public Request<TResponse> Post<TResponse>()
            where TResponse : class
        {
            return Request<TResponse>(HttpMethod.Post);
        }

        /// <summary>
        /// Finalizes request and start it using POST method.
        /// </summary>
        /// <typeparam name="TResult">Expected result type.</typeparam>
        /// <returns>Awaitable request with <typeparamref name="TResult"/> result.</returns>
        public Request<TResponse> Post<TResponse>(CancellationToken token)
            where TResponse : class
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
        public Request<TResponse> Put<TResponse>()
            where TResponse : class
        {
            return Request<TResponse>(HttpMethod.Put);
        }

        /// <summary>
        /// Finalizes request and start it using PUT method.
        /// </summary>
        /// <typeparam name="TResult">Expected result type.</typeparam>
        /// <returns>Awaitable request with <typeparamref name="TResult"/> result.</returns>
        public Request<TResponse> Put<TResponse>(CancellationToken token)
            where TResponse : class
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
        public Request<TResponse> Delete<TResponse>()
            where TResponse : class
        {
            return Request<TResponse>(HttpMethod.Delete);
        }

        /// <summary>
        /// Finalizes request and start it using DELETE method.
        /// </summary>
        /// <typeparam name="TResult">Expected result type.</typeparam>
        /// <returns>Awaitable request with <typeparamref name="TResult"/> result.</returns>
        public Request<TResponse> Delete<TResponse>(CancellationToken token)
            where TResponse : class
        {
            return Request<TResponse>(HttpMethod.Delete, token);
        }

        private Request Request(HttpMethod method)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            return new Request(Service.Request(method, this, source.Token), source);
        }

        private Request Request(HttpMethod method, CancellationToken token)
        {
            CancellationTokenSource requestSource = new CancellationTokenSource();
            CancellationTokenSource source = CancellationTokenSource.CreateLinkedTokenSource(requestSource.Token, token);
            return new Request(Service.Request(method, this, source.Token), requestSource, source);
        }

        private Request<TResponse> Request<TResponse>(HttpMethod method)
            where TResponse : class
        {
            CancellationTokenSource source = new CancellationTokenSource();
            return new Request<TResponse>(Service.Request<TResponse>(method, this, source.Token), source);
        }

        private Request<TResponse> Request<TResponse>(HttpMethod method, CancellationToken token)
            where TResponse : class
        {
            CancellationTokenSource requestSource = new CancellationTokenSource();
            CancellationTokenSource source = CancellationTokenSource.CreateLinkedTokenSource(requestSource.Token, token);
            return new Request<TResponse>(Service.Request<TResponse>(method, this, source.Token), requestSource, source);
        }

        public Uri BuildUrl()
        {
            StringBuilder builder = new StringBuilder()
                .Append(Scheme.ToScheme())
                .Append("://")
                .Append(Host);
            if (Port.HasValue)
                builder.Append(':').Append(Port.Value.ToString());
            foreach (string segment in Segments)
                builder.Append('/').Append(segment);
            bool first = true;
            foreach (QueryParameter param in QueryParameters)
            {
                builder.Append(first ? '?' : '&').Append(param.Key).Append('=').Append(param.Value);
                first = false;
            }
            return new Uri(builder.ToString());
        }

        private HttpContent CreateContent<TRequest>(TRequest body) where TRequest : class
        {
            // Byte array content, File transfer, etc...
            if (body is Stream)
                return new StreamContent(body as Stream);

            // x-form-url-encoded
            if (body is IEnumerable<KeyValuePair<string, string>>)
                return new FormUrlEncodedContent(body as IEnumerable<KeyValuePair<string, string>>);

            // multipart/*
            if (body is IEnumerable<KeyValuePair<string, object>>)
            {
                MultipartFormDataContent content = new MultipartFormDataContent();
                foreach (var kvp in (IEnumerable<KeyValuePair<string, object>>)body)
                {
                    if (kvp.Value is string str)
                        content.Add(new StringContent(str), kvp.Key);
                    else if (kvp.Value is FileContent file)
                        content.Add(CreateContent(file.Stream), kvp.Key, file.FileName);
                    else if (kvp.Value is Stream s)
                        content.Add(CreateContent(s), kvp.Key);
                    else
                        content.Add(CreateContent(kvp.Value), kvp.Key);
                }
                return content;
            }

            // JSON content
            return JsonizeContent(body);
        }

        private HttpContent JsonizeContent<TRequest>(TRequest body) where TRequest : class
        {
            if (body == null)
                return null;

            return Service.Serializer.Serialize(body);
        }
    }
}
