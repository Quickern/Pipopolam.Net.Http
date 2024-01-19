using System.Net;
using Pipopolam.Net.Http.Tests.Common;
using Pipopolam.Net.Http.Tests.ErrorPrehandling.Model;
using RichardSzalay.MockHttp;

namespace Pipopolam.Net.Http.Tests.ErrorPrehandling;

public abstract class AbstractErrorPrehandlingTests(Service<BasicError> service) : AbstractTests<BasicError>(service)
{
    [Fact]
    public async Task SimpleGet()
    {
        Handler.When("https://localhost:2718/service/test_get")
            .With(r => r.Method == HttpMethod.Get)
            .Respond("application/json", "{ \"Success\" : true, \"SomeMessage\" : \"Test message\" }");

        Data data = await Service.CreateRequest().AddSegment("test_get").Get<Data>();
        Assert.Equal("Test message", data.SomeMessage);
    }

    [Fact]
    public async Task ErrorHandling()
    {
        Handler.When("https://localhost:2718/service/test_error")
            .Respond("application/json", "{ \"Code\": 314, \"Message\" : \"Error 314\" }");

        WebServiceErrorException<BasicError> ex = await Assert.ThrowsAsync<WebServiceErrorException<BasicError>>(async () =>
            await Service.CreateRequest().AddSegment("test_error").Post());

        Assert.Equal(314, ex.Response.Code);
        Assert.Equal("Error 314", ex.Response.Message);
    }
}
