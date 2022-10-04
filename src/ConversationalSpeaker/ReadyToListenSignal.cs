namespace ConversationalSpeaker
{
    /// <summary>
    /// A class to notify when the system should be listening.
    /// </summary>
    internal class ReadyToListenSignal : IDisposable
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0, 1);

        /// <summary>
        /// Signal the system to start listening.
        /// </summary>
        public void SetReady() => _semaphore.Release();

        /// <summary>
        /// Wait for a signal to start listening.
        /// </summary>
        public Task WaitForReady(CancellationToken cancellationToken) 
            => _semaphore.WaitAsync(cancellationToken);

        public void Dispose()
            => _semaphore.Dispose();
    }
}
