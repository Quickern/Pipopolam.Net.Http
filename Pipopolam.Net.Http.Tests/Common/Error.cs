using System.Runtime.Serialization;

namespace Pipopolam.Net.Http.Tests.Common;

[DataContract]
public class Error
{
    [DataMember]
    public int Code { get; set; }

    [DataMember]
    public string? Message { get; set; }
}
