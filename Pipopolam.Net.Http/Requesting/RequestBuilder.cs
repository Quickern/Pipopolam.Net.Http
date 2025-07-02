using System;
using System.Collections.Generic;
using System.Text;

namespace Pipopolam.Net.Http.Requesting;

public partial class RequestBuilder : IRequest
{
    public IEnumerable<string> Segments { get; } = new List<string>();
    public IEnumerable<QueryParameter> QueryParameters { get; } = new List<QueryParameter>();
    public IDictionary<string, string?> Headers { get; } = new Dictionary<string, string?>();
    public object? Body { get; private set; }

    public WebService Service { get; }

    internal RequestBuilder(WebService service)
    {
        Service = service;
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
        ((IList<string>)Segments).Add(segment);
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
    public RequestBuilder AddQueryParameter(string key, string? value)
    {
        ((IList<QueryParameter>)QueryParameters).Add(new QueryParameter(key, value));
        return this;
    }

    /// <summary>
    /// Add custom header to the request.
    /// </summary>
    public RequestBuilder AddHeader(string key, string? value)
    {
        Headers[key] = value;
        return this;
    }

    public Uri BuildUrl()
    {
        StringBuilder builder = new StringBuilder();
        //     .Append(Scheme.ToScheme())
        //     .Append("://")
        //     .Append(Host);
        // if (Port.HasValue)
        //     builder.Append(':').Append(Port.Value.ToString());
        // foreach (string segment in Segments)
        //     builder.Append('/').Append(segment);
        // bool first = true;
        // foreach (QueryParameter param in QueryParameters)
        // {
        //     builder.Append(first ? '?' : '&').Append(param.Key).Append('=').Append(param.Value);
        //     first = false;
        // }
        return new Uri(builder.ToString());
    }
}
