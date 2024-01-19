using Pipopolam.Net.Http.Tests.Common;

namespace Pipopolam.Net.Http.Tests.GenericRequests;

public class GenericRequestTests : AbstractGenericRequestTests
{
    public GenericRequestTests() : base(new Service<Error>()) { }
}
