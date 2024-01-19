using Pipopolam.Net.Http.Tests.Common;
using Pipopolam.Net.Http.Tests.ErrorPrehandling.Model;

namespace Pipopolam.Net.Http.Tests.ErrorPrehandling;

public class ErrorPrehandlingTests : AbstractErrorPrehandlingTests
{
    public ErrorPrehandlingTests() : base(new Service<BasicError>()) { }
}
