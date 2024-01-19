using RichardSzalay.MockHttp;

namespace Pipopolam.Net.Http.Tests.Common;

public abstract class AbstractTests<TError> where TError : class
{
    private AutoFlushPreventer _autoFlush;

    protected Service<TError> Service { get; }
    protected MockHttpMessageHandler Handler => Service.Handler;

    protected AbstractTests(Service<TError> service)
    {
        Service = service;
        _autoFlush = new AutoFlushPreventer(this);
    }

    protected IDisposable PreventAutoFlush() => _autoFlush.Prevent();

    private class AutoFlushPreventer(AbstractTests<TError> tests) : IDisposable
    {
        public AutoFlushPreventer Prevent()
        {
            tests.Handler.AutoFlush = false;
            return this;
        }

        public void Dispose()
        {
            tests.Handler.AutoFlush = true;
        }
    }
}
