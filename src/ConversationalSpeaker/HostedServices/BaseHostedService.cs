using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConversationalSpeaker
{
    /// <summary>
    /// Base class for hosted services.
    /// </summary>
    internal abstract class BaseHostedService<T> : IHostedService, IDisposable
    {
        protected readonly ILogger<T> _logger;
        private Task _executeTask;
        private readonly CancellationTokenSource _cancelToken = new();

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseHostedService(
            ILogger<T> logger,
            IHostApplicationLifetime appLifetime)
        {
            _logger = logger;
            appLifetime.ApplicationStarted.Register(OnStarted);
            appLifetime.ApplicationStopping.Register(OnStopping);
            appLifetime.ApplicationStopped.Register(OnStopped);
        }

        /// <summary>
        /// Start the service.
        /// </summary>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _executeTask = ExecuteAsync(_cancelToken.Token);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Primary logic loop.
        /// </summary>
        public abstract Task ExecuteAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Stop a running service.
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancelToken.Cancel();
            return Task.CompletedTask;
        }

        // Report service statuses
        private void OnStarted() 
            => _logger.LogInformation($"{this.GetType().Name} started.");

        private void OnStopping() 
            => _logger.LogInformation($"{this.GetType().Name} stopping.");

        private void OnStopped() 
            => _logger.LogInformation($"{this.GetType().Name} stopped.");

        public virtual void Dispose()
        {
            _cancelToken.Dispose();
        }
    }
}
