using Pipopolam.Net.Http.Serialization;
using Pipopolam.Net.Http.Tests.Common;

namespace Pipopolam.Net.Http.Tests;

public class DataContactTests : AbstractGenericRequestTests
{
    public DataContactTests() : base(new Service(() => new DataContractSerializer())) { }
}
