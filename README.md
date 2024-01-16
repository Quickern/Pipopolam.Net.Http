# Pipopolam.Net Http web client library

[![NuGet Status](https://img.shields.io/nuget/v/Pipopolam.Net.Http)](https://www.nuget.org/packages/Pipopolam.Net.Http/)

## Example

    [DataContract]
    public class RemoteError
    {
        [DataMember]
        public int Code { get; set; }

        [DataMember]
        public string Message { get; set; }
    }

    public class MyService : WebService<RemoteError>
    {
        public override string BaseHost => "example.com";

        protected override void GenericServicePath(RequestBuilder builder) => builder.AddSegment("service");
    }

    [DataContract]
    public class MyObject
    {
        [DataMember]
        public string SomeString { get; set; }
    }

    // Usage
    public async Task TestRequest()
    {
        // Get service implementation:
        MyService service = Container.Resolve<MyService>();

        // Request:
        try
        {
            MyObject obj = await service.CreateRequest().AddSegment("object").Get<MyObject>();
            Console.WriteLine(@"Got: {obj.SomeString}!");
        }
        catch (WebServiceRemoteException<RemoteError> ex)
        {
            Console.WriteLine(@"Something went wrong!\n{ex.Response.Message}");
        }
    }