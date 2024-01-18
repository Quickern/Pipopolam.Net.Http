using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Pipopolam.Net.Http.Serialization
{
    public class NetJsonSerializer : ISerializer
    {
        public HttpContent Serialize<T>(T obj) where T : class
        {
            return new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");
        }

        public async Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken) where T : class
        {
            return await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: cancellationToken);
        }
    }
}
