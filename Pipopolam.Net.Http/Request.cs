using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Pipopolam.Net.Http
{
    public class Request
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Task _task;

        protected virtual Task Task => _task;

        public HttpResponseHeaders Headers { get; protected set; }

        protected Request(CancellationTokenSource cancellationTokenSource)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }

        internal Request(Task<ServiceResponse> task, CancellationTokenSource cancellationTokenSource):
            this(cancellationTokenSource)
        {
            _task = RequestWrapper(task);
        }

        public TaskAwaiter GetAwaiter() => Task.GetAwaiter();

        private async Task RequestWrapper(Task<ServiceResponse> task)
        {
            ServiceResponse response = await task;

            Headers = response.Headers;
        }

        public void Cancel() => _cancellationTokenSource.Cancel();

        public static implicit operator Task(Request request)
        {
            return request.Task;
        }
    }

    public class Request<T> : Request where T: class
    {
        private readonly Task<T> _task;

        protected override Task Task => _task;

        public T Result => _task.Result;

        internal Request(Task<ServiceResponse<T>> task, CancellationTokenSource cancellationTokenSource) :
            base(cancellationTokenSource)
        {
            this._task = RequestWrapper(task);
        }

        public new TaskAwaiter<T> GetAwaiter() => _task.GetAwaiter();

        private async Task<T> RequestWrapper(Task<ServiceResponse<T>> task)
        {
            ServiceResponse<T> response = await task;

            Headers = response.Headers;

            return response?.Data;
        }

        public static implicit operator Task<T>(Request<T> request)
        {
            return request._task;
        }
    }
}
