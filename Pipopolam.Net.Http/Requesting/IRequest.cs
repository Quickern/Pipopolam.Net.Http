using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Pipopolam.Net.Http.Requesting;

public interface IRequest
{
    Task Request(HttpMethod method);
    Task Request(HttpMethod method, CancellationToken token);
    Task<TResponse> Request<TResponse>(HttpMethod method);
    Task<TResponse> Request<TResponse>(HttpMethod method, CancellationToken token);
}
