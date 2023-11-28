using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace Pipopolam.Net
{
    public class RequestBuilder
    {
        public UrlScheme Scheme { get; private set; } = UrlScheme.Http;
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

        public RequestBuilder AddSegment(string segment)
        {
            if (String.IsNullOrWhiteSpace(segment))
                throw new InvalidOperationException("Empty url segment");
            (Segments as IList<string>).Add(segment);
            return this;
        }

        public RequestBuilder AddPath(string segments)
        {
            string[] s = segments.Split('/');
            foreach (string segment in s)
                AddSegment(segment);
            return this;
        }

        public RequestBuilder AddQueryParameter(string key, string value)
        {
            (QueryParameters as IList<QueryParameter>).Add(new QueryParameter(key, value));
            return this;
        }

        public RequestBuilder AddHeader(string key, string value)
        {
            Headers[key] = value;
            return this;
        }

        public RequestBuilder Body<TRequest>(TRequest body) where TRequest : class
        {
            Content = CreateContent(body);
            return this;
        }

        public Request<TResult> Get<TResult>() where TResult : class
        {
            return Request<TResult>(HttpMethod.Get);
        }

        public Request Post()
        {
            return Request(HttpMethod.Post);
        }

        public Request<TResponse> Post<TResponse>()
            where TResponse : class
        {
            return Request<TResponse>(HttpMethod.Post);
        }

        public Request Put()
        {
            return Request(HttpMethod.Put);
        }

        public Request<TResponse> Put<TResponse>()
            where TResponse : class
        {
            return Request<TResponse>(HttpMethod.Put);
        }

        public Request Delete()
        {
            return Request(HttpMethod.Delete);
        }

        public Request<TResponse> Delete<TResponse>()
            where TResponse : class
        {
            return Request<TResponse>(HttpMethod.Delete);
        }

        private Request Request(HttpMethod method)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            return new Request(Service.Request(method, this, source.Token), source);
        }

        private Request<TResponse> Request<TResponse>(HttpMethod method)
            where TResponse : class
        {
            CancellationTokenSource source = new CancellationTokenSource();
            return new Request<TResponse>(Service.Request<TResponse>(method, this, source.Token), source);
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
