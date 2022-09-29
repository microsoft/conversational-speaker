using System.Collections.Concurrent;

namespace ConversationalSpeaker
{
    /// <summary>
    /// A thread/task-safe message queue.
    /// </summary>
    /// <typeparam name="T">The type of message.</typeparam>
    internal class ConcurrentMessageQueue<T> : IDisposable
    {
        private readonly SemaphoreSlim _signal = new(1);
        private readonly ConcurrentQueue<T> _queue = new();

        /// <summary>
        /// Enqueue a new item of type T.
        /// </summary>
        public void Enqueue(T item)
        {
            _queue.Enqueue(item);
            _signal.Release();
        }

        /// <summary>
        /// Try to dequeue an item from the queue.
        /// </summary>
        public bool TryDequeue(out T item) 
            => _queue.TryDequeue(out item);

        /// <summary>
        /// Wait for a new item to be enqueued.
        /// </summary>
        public async Task Wait(CancellationToken cancelToken) 
            => await _signal.WaitAsync(cancelToken);

        public void Dispose() 
            => _signal.Dispose();
    }
}
