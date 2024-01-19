using Pipopolam.Net.Http.Serialization;
using Pipopolam.Net.Http.Tests.Common;

namespace Pipopolam.Net.Http.Tests.GenericRequests;

public class DataContractGenericRequestsTests : AbstractGenericRequestTests
{
    public DataContractGenericRequestsTests() : base(new Service<Error>(() => new DataContractSerializer())) { }
}
