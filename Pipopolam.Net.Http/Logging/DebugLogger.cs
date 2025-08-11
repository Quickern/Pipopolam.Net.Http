using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pipopolam.Net.Http.Logging;

public class DebugLogger : IWebServiceLogger
{
    public bool IsFullLogEnabled { get; init; }

    public async ValueTask LogRequest(int id, HttpRequestMessage request)
    {
        string? body = null;

        if (IsFullLogEnabled && request.Content != null)
        {
            body = await request.Content.ReadAsStringAsync();
        }

        if (body == null)
        {
            Debug.WriteLine($"[WebService]({id}) Request: {request.RequestUri}");
        }
        else
        {
            Debug.WriteLine($"[WebService]({id}) Request: {request.RequestUri}\n{body}");
        }
    }

    public async ValueTask LogResponse(int id, HttpResponseMessage response)
    {
        string? body = null;

        if (IsFullLogEnabled)
        {
            body = await response.Content.ReadAsStringAsync();
        }

        if (body == null)
        {
            Debug.WriteLine($"[WebService]({id}) Response: {response.StatusCode}");
        }
        else
        {
            Debug.WriteLine($"[WebService]({id}) Response: {response.StatusCode}\n{body}");
        }
    }

    public void LogError(int id, string message, Exception exception)
    {
        Debug.WriteLine($"[WebService]({id}) Error: {message}\n{exception}");
    }
}
