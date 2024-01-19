using System;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Pipopolam.Net.Http
{
    public class Request : IDisposable
    {
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationTokenSource _linkedSource;

        private protected Task Task { get; set; }

        public HttpResponseHeaders Headers { get; protected set; }

        private protected Request(CancellationTokenSource cancellationTokenSource, CancellationTokenSource linkedSource)
        {
            _cancellationTokenSource = cancellationTokenSource;
            _linkedSource = linkedSource;
        }

        internal Request(Task<ServiceResponse> task, CancellationTokenSource cancellationTokenSource, CancellationTokenSource linkedSource = null) :
            this(cancellationTokenSource, linkedSource)
        {
            Task = RequestWrapper(task);
        }

        public TaskAwaiter GetAwaiter() => Task.GetAwaiter();

        private async Task RequestWrapper(Task<ServiceResponse> task)
        {
            try
            {
                ServiceResponse response = await task;

                Headers = response.Headers;
            }
            finally
            {
                Clear();
            }
        }

        private protected void Clear()
        {
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;

            _linkedSource?.Dispose();
            _linkedSource = null;
        }

        /// <summary>
        /// Cancels request.
        /// </summary>
        public void Cancel() => _cancellationTokenSource?.Cancel();

        /// <summary>
        /// Converts request to task.
        /// </summary>
        public Task ToTask() => Task;

        public void Dispose() => Cancel();

        public static implicit operator Task(Request request)
        {
            return request.ToTask();
        }
    }

    public class Request<T> : Request where T: class
    {
        private new Task<T> Task { get; }

        public T Result => Task.Result;

        internal Request(Task<ServiceResponse<T>> task, CancellationTokenSource cancellationTokenSource, CancellationTokenSource linkedSource = null) :
            base(cancellationTokenSource, linkedSource)
        {
            base.Task = Task = RequestWrapper(task);
        }

        public new TaskAwaiter<T> GetAwaiter() => Task.GetAwaiter();

        private async Task<T> RequestWrapper(Task<ServiceResponse<T>> task)
        {
            try
            {
                ServiceResponse<T> response = await task;

                Headers = response.Headers;

                return response?.Data;
            }
            finally
            {
                Clear();
            }
        }

        /// <summary>
        /// Converts request to task.
        /// </summary>
        public new Task<T> ToTask() => Task;

        public static implicit operator Task<T>(Request<T> request)
        {
            return request.ToTask();
        }
    }
}
