using System;
using System.Net;

namespace Pipopolam.Net.Http
{
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

    public class WebServiceErrorException : WebServiceException
    {
        public string? Response { get; private set; }

        public WebServiceErrorException(string? response)
        {
            Response = response;
        }
    }

    public class WebServiceErrorException<T> : WebServiceException
        where T: class
    {
        public T Response { get; private set; }

        public WebServiceErrorException(T response)
        {
            Response = response;
        }
    }

    public class WebServiceRemoteException : WebServiceErrorException
    {
        public HttpStatusCode StatusCode { get; private set; }

        public WebServiceRemoteException(HttpStatusCode statusCode, string? response) :
            base(response)
        {
            StatusCode = statusCode;
        }
    }

    public class WebServiceRemoteException<T> : WebServiceErrorException<T> where T: class
    {
        public HttpStatusCode StatusCode { get; private set; }

        public WebServiceRemoteException(HttpStatusCode statusCode, T response) :
            base(response)
        {
            StatusCode = statusCode;
        }
    }
}
