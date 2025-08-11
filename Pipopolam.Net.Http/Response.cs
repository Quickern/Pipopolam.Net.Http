using System.Collections.Generic;
using System.Net;

namespace Pipopolam.Net.Http;

public record Response<TError>(HttpStatusCode StatusCode, Dictionary<string, string> Headers, TError? error = default);

public record Response<TResult, TError>(HttpStatusCode StatusCode,
    Dictionary<string, string> Headers,
    TResult Result,
    TError? Error = default) : Response<TError>(StatusCode, Headers, Error);

/// <summary>
/// Error for services with error prehandling need to implement <see cref="IBasicResponse" /> interface
/// </summary>
public interface IBasicResponse
{
    bool IsSuccess { get; }
}
