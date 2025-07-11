using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pipopolam.Net.Http.Serialization;

public class DataContractSerializer : ISerializer
{
    public DataContractJsonSerializerSettings? Settings { get; }

    public DataContractSerializer() { }

    public DataContractSerializer(DataContractJsonSerializerSettings settings)
    {
        Settings = settings;
    }

    [return: NotNullIfNotNull(nameof(obj))]
    public HttpContent? Serialize<T>(T? obj)
    {
        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T), Settings);
        using (MemoryStream stream = new MemoryStream())
        {
            ser.WriteObject(stream, obj);
            byte[] arr = stream.ToArray();
            string t = Encoding.UTF8.GetString(arr, 0, arr.Length);
            return new StringContent(t, Encoding.UTF8, "application/json");
        }
    }

    public Task<T?> DeserializeAsync<T>(HttpContent content, CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T), Settings);
            object? obj = ser.ReadObject(await content.ReadAsStreamAsync().ConfigureAwait(false));
            return obj == null ? default : (T)obj;
        }, cancellationToken);
    }
}
