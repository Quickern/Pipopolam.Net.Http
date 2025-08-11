namespace Pipopolam.Net.Http;

public interface IWebService
{
    IRequestBuilder CreateRequest();
    void Close();
}
