using System.Net;
using Pipopolam.Net.Http.Tests.Common;
using RichardSzalay.MockHttp;

namespace Pipopolam.Net.Http.Tests;

public abstract class AbstractGenericRequestTests
{
    private readonly Service _service;

    protected MockHttpMessageHandler Handler => _service.Handler;

    protected AbstractGenericRequestTests(Service service)
    {
        _service = service;
    }

    [Fact]
    public async Task SimpleGet()
    {
        Handler.When("https://localhost:2718/service/test_get")
            .Respond("application/json", "{ \"SomeMessage\" : \"Test message\" }");

        Data data = await _service.CreateRequest().AddSegment("test_get").Get<Data>();
        Assert.Equal("Test message", data.SomeMessage);
    }

    [Fact]
    public async Task GetArray()
    {
        Handler.When("https://localhost:2718/service/test_get_array")
            .Respond("application/json", "[ { \"SomeMessage\" : \"Test message\" },  { \"SomeMessage\" : \"Test message 2\" } ]");

        Data[] data = await _service.CreateRequest().AddSegment("test_get_array").Get<Data[]>();
        Assert.Equal(2, data.Length);
        Assert.Equal("Test message", data[0].SomeMessage);
        Assert.Equal("Test message 2", data[1].SomeMessage);

        List<Data> dataList = await _service.CreateRequest().AddSegment("test_get_array").Get<List<Data>>();
        Assert.Equal(2, dataList.Count);
        Assert.Equal("Test message", dataList[0].SomeMessage);
        Assert.Equal("Test message 2", dataList[1].SomeMessage);
    }

    [Fact]
    public async Task ErrorHandling()
    {
        Handler.When("https://localhost:2718/service/test_error")
            .Respond(HttpStatusCode.BadRequest, "application/json", "{ \"Code\": 314, \"Message\" : \"Error 314\" }");

        WebServiceRemoteException<Error> ex = await Assert.ThrowsAsync<WebServiceRemoteException<Error>>(async () =>
            await _service.CreateRequest().AddSegment("test_error").Post());

        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.Equal(314, ex.Response.Code);
        Assert.Equal("Error 314", ex.Response.Message);
    }

    [Fact]
    public async Task Cancellation()
    {
        Handler.When("https://localhost:2718/service/test_cancel")
            .Respond("application/json", "{ \"SomeMessage\" : \"Test message\" }");

        Handler.AutoFlush = false;

        Request request = _service.CreateRequest().AddSegment("test_cancel").Get<Data>();

        async Task CancelAndFlush()
        {
            await Task.Yield();
            request.Cancel();
            Handler.Flush();
        }

        await Task.WhenAll(Assert.ThrowsAsync<TaskCanceledException>(async () => await request), CancelAndFlush());

        Handler.AutoFlush = true;
    }
}
