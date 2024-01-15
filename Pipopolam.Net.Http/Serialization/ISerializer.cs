using System;
using System.IO;
using System.Net.Http;

namespace Pipopolam.Net.Http.Serialization
{
    public interface ISerializer
    {
        HttpContent Serialize<T>(T obj) where T : class; // TODO: Remove this strange restriction
        T Deserialize<T>(Stream stream) where T : class;
    }
}
