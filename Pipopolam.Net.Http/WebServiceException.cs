using System;
using System.Net;

namespace Pipopolam.Net.Http;

public class WebServiceException : Exception
{
    public WebServiceException() { }

    public WebServiceException(string? message) : base(message) { }

    public WebServiceException(string? message, Exception? innerException) : base(message, innerException) { }
}

public class WebServiceNoConnectionException : WebServiceException
{
    public WebServiceNoConnectionException(Exception? innerException) : base("Can't connect to service", innerException) { }
}

public class WebServiceRemoteException : WebServiceException
{
    public HttpStatusCode StatusCode { get; }

    public WebServiceRemoteException(HttpStatusCode statusCode)
    {
        StatusCode = statusCode;
    }
}

public class WebServiceRemoteException<T> : WebServiceRemoteException
{
    public T Response { get; }

    public WebServiceRemoteException(HttpStatusCode statusCode, T response) : base(statusCode)
    {
        Response = response;
    }
}
