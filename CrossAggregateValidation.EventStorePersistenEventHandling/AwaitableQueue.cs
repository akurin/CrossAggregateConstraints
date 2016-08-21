using System.Collections.Generic;
using System.Threading.Tasks;

namespace ESUtils.PersistentSubscription
{
    internal class AwaitableQueue<T>
    {
        private readonly Queue<T> _queue = new Queue<T>();
        private readonly Queue<TaskCompletionSource<T>> _waiting = new Queue<TaskCompletionSource<T>>();

        public void Send(T item)
        {
            TaskCompletionSource<T> tcs = null;
            lock (_queue)
            {
                if (_waiting.Count > 0)
                    tcs = _waiting.Dequeue();
                else
                    _queue.Enqueue(item);
            }

            tcs?.TrySetResult(item);
        }

        public Task<T> TakeAsync()
        {
            lock (_queue)
            {
                if (_queue.Count > 0)
                {
                    return Task.FromResult(_queue.Dequeue());
                }

                var tcs = new TaskCompletionSource<T>();
                _waiting.Enqueue(tcs);
                return tcs.Task;
            }
        }
    }

    internal class AwaitableQueue : AwaitableQueue<object>
    {
    }
}