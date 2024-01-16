
using RichardSzalay.MockHttp;

namespace Pipopolam.Net.Http.Tests.Common;

public class Service : WebService<Error>
{
    private MockHttpMessageHandler _handler;

    public override string BaseHost => "localhost:2718";
    protected override HttpMessageHandler CreateHandler() => _handler;

    public Service(MockHttpMessageHandler handler)
    {
        _handler = handler;
    }

    protected override void GenericServicePath(RequestBuilder builder)
    {
        builder.AddSegment("service");
    }
}
