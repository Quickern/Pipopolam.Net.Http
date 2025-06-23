using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;

namespace Pipopolam.Net.Http.Requesting;

partial class RequestBuilder
{
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
    public RequestBuilder Body<TRequest>(TRequest? body) where TRequest : class
    {
        Content = CreateContent(body);
        return this;
    }


    [return: NotNullIfNotNull(nameof(body))]
    private HttpContent? CreateContent<TRequest>(TRequest? body) where TRequest : class
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

    [return: NotNullIfNotNull(nameof(body))]
    private HttpContent? JsonizeContent<TRequest>(TRequest? body) where TRequest : class
    {
        if (body == null)
            return null;

        return Service.Serializer.Serialize(body);
    }
}
