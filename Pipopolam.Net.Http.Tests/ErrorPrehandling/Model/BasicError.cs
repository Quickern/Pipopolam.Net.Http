using System.Runtime.Serialization;
using Pipopolam.Net.Http.Tests.Common;

namespace Pipopolam.Net.Http.Tests.ErrorPrehandling.Model;

[DataContract]
public class BasicError : Error, IBasicResponse
{
    [DataMember]
    public bool Success { get; set; }
}
