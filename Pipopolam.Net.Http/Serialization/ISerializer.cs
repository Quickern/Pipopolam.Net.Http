using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Pipopolam.Net.Http.Serialization
{
    public interface ISerializer
    {
        HttpContent Serialize<T>(T obj) where T : class; // TODO: Remove this strange restriction
        Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken) where T : class;
    }
}
