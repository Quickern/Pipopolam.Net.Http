using System.Runtime.Serialization;

namespace Pipopolam.Net.Http.Tests;

[DataContract]
public class Data
{
    [DataMember]
    public string? SomeMessage { get; set; }
}
