using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pipopolam.Net.Http.Logging;

public interface IWebServiceLogger
{
    ValueTask LogRequest(int id, HttpRequestMessage request);
    ValueTask LogResponse(int id, HttpResponseMessage response);

    void LogError(int id, string message, Exception exception);
}
