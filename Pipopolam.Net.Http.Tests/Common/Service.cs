
using Pipopolam.Net.Http.Serialization;
using RichardSzalay.MockHttp;

namespace Pipopolam.Net.Http.Tests.Common;

public class Service<TError> : WebService<TError> where TError : class
{
    private readonly Func<ISerializer>? _serializerFactory;

    public MockHttpMessageHandler Handler { get; } = new MockHttpMessageHandler();

    public override string BaseHost => "localhost:2718";
    protected override HttpMessageHandler CreateHandler() => Handler;
    protected override ISerializer CreateSerializer() => _serializerFactory?.Invoke() ?? base.CreateSerializer();

    public Service(Func<ISerializer>? serializerFactory = null)
    {
        _serializerFactory = serializerFactory;
    }

    protected override void GenericServicePath(RequestBuilder builder)
    {
        builder.AddSegment("service");
    }
}
