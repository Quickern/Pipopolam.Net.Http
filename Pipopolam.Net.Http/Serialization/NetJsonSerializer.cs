using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Pipopolam.Net.Http.Serialization;

public class NetJsonSerializer : ISerializer
{
    [return: NotNullIfNotNull(nameof(obj))]
    public HttpContent? Serialize<T>(T? obj)
    {
        return new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");
    }

    public async Task<T?> DeserializeAsync<T>(HttpContent content, CancellationToken cancellationToken)
    {
        return await JsonSerializer.DeserializeAsync<T>(await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false), cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
