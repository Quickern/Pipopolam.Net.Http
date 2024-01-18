# Pipopolam.Net Http Helper Library

Main goal of this library is to simplify client code  HTTP-based APIs requests (e.g. REST, etc.)

[![NuGet Status](https://img.shields.io/nuget/vpre/Pipopolam.Net.Http)](https://www.nuget.org/packages/Pipopolam.Net.Http/)
[![Build](https://github.com/Quickern/Pipopolam.Net.Http/actions/workflows/build.yml/badge.svg)](https://github.com/Quickern/Pipopolam.Net.Http/actions/workflows/build.yml)

## Example

First of all we need to create subclass for `WebService` and configure it.

```csharp
public class RemoteError
{
    public int Code { get; set; }
    public string Message { get; set; }
}

// In case of server error (not 200(OK) response)
// try to parse server response into RemoteError object
public class MyService : WebService<RemoteError>
{
    // All requests uses "example.com" host
    public override string BaseHost => "example.com";

    // All requests starts with "https://example.com/service
    protected override void GenericServicePath(RequestBuilder builder) => builder.AddSegment("service");
}
```

> Note that for .netcore and .net platforms `System.Text.Json` is used for serialization by default. For other platforms `DataContractJsonSerializer` will be used.

Now we have class `MyService` and can use it.

```csharp
public class MyObject
{
    public string SomeString { get; set; }
}

// Get service implementation, for example:
// MyService _service = Container.Resolve<MyService>();

// Usage
public async Task RequestObject()
{
    try
    {
        // Make GET-request to https://example.com/service/object
        MyObject obj = await _service.CreateRequest().AddSegment("object").Get<MyObject>();

        Console.WriteLine($"Got: {obj.SomeString}!");
    }
    catch (WebServiceRemoteException<RemoteError> ex)
    {
        Console.WriteLine($"Something went wrong!\n{ex.Response.Message}");
    }
}

public async Task PostObject()
{
    try
    {
        // Make POST-request to https://example.com/service/object/set
        // and set new object as request body
        await _service.CreateRequest()
                      .AddSegment("object")
                      .AddSegment("set")
                      .Body(new MyObject { SomeString = "Hello, world!" })
                      .Post();
    }
    catch (WebServiceRemoteException<RemoteError> ex)
    {
        Console.WriteLine($"Something went wrong!\n{ex.Response.Message}");
    }
}
```
