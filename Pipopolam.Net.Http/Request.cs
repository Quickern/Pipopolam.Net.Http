using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Pipopolam.Net.Http
{
    public class Request : IDisposable
    {
        private CancellationTokenSource? _cancellationTokenSource;
        private CancellationTokenSource? _linkedSource;

        private Task<Response> _originalTask;

        private protected Task Task => _originalTask;

        public Response Response => _originalTask.Result;

        internal Request(Task<Response> task, CancellationTokenSource cancellationTokenSource, CancellationTokenSource? linkedSource = null)
        {
            _cancellationTokenSource = cancellationTokenSource;
            _linkedSource = linkedSource;

            _originalTask = task.ContinueWith(t =>
            {
                Clear();

                return t.Result;
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        public TaskAwaiter GetAwaiter() => Task.GetAwaiter();

        private protected void Clear()
        {
            _cancellationTokenSource?.Dispose();
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

    public class Request<T> : Request
    {
        private new Task<T> Task

        public T Result => Task.Result;

        internal Request(Task<Response<T>> task, CancellationTokenSource cancellationTokenSource, CancellationTokenSource? linkedSource = null) :
            base(task.ContinueWith<Response>(t => t.Result, TaskContinuationOptions.ExecuteSynchronously), cancellationTokenSource, linkedSource)
        {
            // base.Task = Task = RequestWrapper(task);
        }

        public new TaskAwaiter<T> GetAwaiter() => Task.GetAwaiter();

        /// <summary>
        /// Converts request to task.
        /// </summary>
        public new Task<T> ToTask() => Task;

        public static implicit operator Task<T?>(Request<T> request)
        {
            return request.ToTask();
        }
    }
}
