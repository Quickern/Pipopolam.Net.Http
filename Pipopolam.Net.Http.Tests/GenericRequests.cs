using System.Net;
using Pipopolam.Net.Http.Tests.Common;
using RichardSzalay.MockHttp;

namespace Pipopolam.Net.Http.Tests;

public class GenericRequests
{
    private MockHttpMessageHandler _handler;
    private readonly Service _service;

    public GenericRequests()
    {
        _handler = new MockHttpMessageHandler();
        _service = new Service(_handler);
    }

    [Fact]
    public async Task SimpleGet()
    {
        _handler.When("https://localhost:2718/service/test_get")
            .Respond("application/json", "{ \"SomeMessage\" : \"Test message\" }");

        Data data = await _service.CreateRequest().AddSegment("test_get").Get<Data>();
        Assert.Equal("Test message", data.SomeMessage);
    }

    [Fact]
    public async Task ErrorHandling()
    {
        _handler.When("https://localhost:2718/service/test_error")
            .Respond(HttpStatusCode.BadRequest, "application/json", "{ \"Code\": 314, \"Message\" : \"Error 314\" }");

        WebServiceRemoteException<Error> ex = await Assert.ThrowsAsync<WebServiceRemoteException<Error>>(async () =>
            await _service.CreateRequest().AddSegment("test_error").Post());
        
        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.Equal(314, ex.Response.Code);
        Assert.Equal("Error 314", ex.Response.Message);
    }
}