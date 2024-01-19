using Pipopolam.Net.Http.Serialization;
using Pipopolam.Net.Http.Tests.Common;
using Pipopolam.Net.Http.Tests.ErrorPrehandling.Model;

namespace Pipopolam.Net.Http.Tests.ErrorPrehandling;

public class DataContractErrorPrehandlingTests : AbstractErrorPrehandlingTests
{
    public DataContractErrorPrehandlingTests() : base(new Service<BasicError>(() => new DataContractSerializer())) { }
}
