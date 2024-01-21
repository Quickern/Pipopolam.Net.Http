using System.Net;
using Pipopolam.Net.Http.Tests.Common;
using RichardSzalay.MockHttp;

namespace Pipopolam.Net.Http.Tests.GenericRequests;

public abstract class AbstractGenericRequestTests(Service<Error> service) : AbstractTests<Error>(service)
{
    [Fact]
    public async Task SimpleGet()
    {
        Handler.When("https://localhost:2718/service/test_get")
            .With(r => r.Method == HttpMethod.Get)
            .Respond("application/json", "{ \"SomeMessage\" : \"Test message\" }");

        Data? data = await Service.CreateRequest().AddSegment("test_get").Get<Data>();
        Assert.NotNull(data);
        Assert.Equal("Test message", data.SomeMessage);
    }

    [Fact]
    public async Task SimplePost()
    {
        Data data = new Data { SomeMessage = "Sent message" };

        Handler.When("https://localhost:2718/service/test_post")
            .With(r => r.Method == HttpMethod.Post)
            .WithJsonContent(data)
            .Respond("application/json", "{ \"SomeMessage\" : \"Test message\" }");

        Data? result = await Service.CreateRequest().AddSegment("test_post").Body(data).Post<Data>();
        Assert.NotNull(result);
        Assert.Equal("Test message", result.SomeMessage);
    }

    [Fact]
    public async Task GetArray()
    {
        Handler.When("https://localhost:2718/service/test_get_array")
            .With(r => r.Method == HttpMethod.Get)
            .Respond("application/json", "[ { \"SomeMessage\" : \"Test message\" },  { \"SomeMessage\" : \"Test message 2\" } ]");

        Data[]? data = await Service.CreateRequest().AddSegment("test_get_array").Get<Data[]>();
        Assert.NotNull(data);
        Assert.Equal(2, data.Length);
        Assert.Equal("Test message", data[0].SomeMessage);
        Assert.Equal("Test message 2", data[1].SomeMessage);
    }

    [Fact]
    public async Task GetList()
    {
        Handler.When("https://localhost:2718/service/test_get_array")
            .With(r => r.Method == HttpMethod.Get)
            .Respond("application/json", "[ { \"SomeMessage\" : \"Test message\" },  { \"SomeMessage\" : \"Test message 2\" } ]");

        List<Data>? dataList = await Service.CreateRequest().AddSegment("test_get_array").Get<List<Data>>();
        Assert.NotNull(dataList);
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
            await Service.CreateRequest().AddSegment("test_error").Post());

        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.Equal(314, ex.Response.Code);
        Assert.Equal("Error 314", ex.Response.Message);
    }

    [Fact]
    public async Task IncorrectErrorHandling()
    {
        Handler.When("https://localhost:2718/service/test_error")
            .Respond(HttpStatusCode.BadRequest, "application/json", "Not OK");

        WebServiceRemoteException ex = await Assert.ThrowsAsync<WebServiceRemoteException>(async () =>
            await Service.CreateRequest().AddSegment("test_error").Post());

        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.Equal("Not OK", ex.Response);
    }

    [Fact]
    public async Task Cancellation()
    {
        Handler.When("https://localhost:2718/service/test_cancel/by_request")
            .Respond("application/json", "{ \"SomeMessage\" : \"Test message\" }");

        using (PreventAutoFlush())
        {
            using Request request = Service.CreateRequest()
                .AddSegment("test_cancel")
                .AddSegment("by_request")
                .Get<Data>();

            async Task CancelAndFlush()
            {
                await Task.Yield();
                request.Cancel();
                Handler.Flush();
            }

            await Task.WhenAll(Assert.ThrowsAsync<TaskCanceledException>(() => request), CancelAndFlush());
        }
    }

    [Fact]
    public async Task CancellationToken()
    {
        Handler.When("https://localhost:2718/service/test_cancel/by_token")
            .Respond("application/json", "{ \"SomeMessage\" : \"Test message\" }");

        using (PreventAutoFlush())
        {
            using CancellationTokenSource tcs = new CancellationTokenSource();

            using Request request = Service.CreateRequest()
                .AddSegment("test_cancel")
                .AddSegment("by_token")
                .Get<Data>(tcs.Token);

            async Task CancelAndFlush()
            {
                await Task.Yield();
                tcs.Cancel();
                Handler.Flush();
            }

            await Task.WhenAll(Assert.ThrowsAsync<TaskCanceledException>(() => request), CancelAndFlush());
        }
    }

    [Fact]
    public async Task AllRequestsCancellation()
    {
        Handler.When("https://localhost:2718/service/test_cancel/all")
            .Respond("application/json", "{ \"SomeMessage\" : \"Test message\" }");

        using (PreventAutoFlush())
        {
            RequestBuilder builder = Service.CreateRequest()
                .AddSegment("test_cancel")
                .AddSegment("all");

            Request[] requests = [ builder.Get<Data>(), builder.Get<Data>(), builder.Get<Data>() ];

            async Task CancelAndFlush()
            {
                await Task.Yield();
                Service.Close();
                Handler.Flush();
            }

            await Task.WhenAll(requests.Select(r => Assert.ThrowsAsync<TaskCanceledException>(() => r))
                .Append(CancelAndFlush()));

            foreach (Request request in requests)
                request.Dispose();
        }
    }
}
