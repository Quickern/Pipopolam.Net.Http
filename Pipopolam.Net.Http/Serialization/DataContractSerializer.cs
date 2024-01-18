using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pipopolam.Net.Http.Serialization
{
    public class DataContractSerializer : ISerializer
    {
        public DataContractJsonSerializerSettings Settings { get; }

        public DataContractSerializer() { }

        public DataContractSerializer(DataContractJsonSerializerSettings settings)
        {
            Settings = settings;
        }

        public HttpContent Serialize<T>(T obj) where T : class
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

        public Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken) where T : class
        {
            return Task.Run(() =>
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T), Settings);
                return ser.ReadObject(stream) as T;
            }, cancellationToken);
        }
    }
}
