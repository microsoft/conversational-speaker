using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConversationalSpeaker
{
    /// <summary>
    /// A hosted service providing the primary conversation loop.
    /// </summary>
    internal class ConversationLoopHostedService : IHostedService, IDisposable
    {
        private readonly AzCognitiveServicesListener _listener;
        private readonly AzCognitiveServicesSpeaker _speaker;
        private readonly PromptEngineHandler _conversationHandler;
        private readonly ILogger<ConversationLoopHostedService> _logger;

        private Task _executeTask;
        private readonly CancellationTokenSource _cancelToken = new();

        /// <summary>
        /// Constructor
        /// </summary>
        public ConversationLoopHostedService(
            AzCognitiveServicesListener listener,
            AzCognitiveServicesSpeaker speaker,
            PromptEngineHandler conversationHandler,
            ILogger<ConversationLoopHostedService> logger)
        {
            _listener = listener;
            _speaker = speaker;
            _conversationHandler = conversationHandler;
            _logger = logger;
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
        /// Primary service logic loop.
        /// </summary>
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // Say hello on startup
            await _speaker.SpeakAsync("Hello!", cancellationToken);

            // Start listening
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Listening...");
                string userMessage = await _listener.ListenAsync(cancellationToken);

                // Run what the user said through the conversation handler (i.e. AI)
                string response = await _conversationHandler.ProcessAsync(userMessage, cancellationToken);

                // Speak the response from the AI, if any.
                if (!string.IsNullOrWhiteSpace(response))
                {
                    await _speaker.SpeakAsync(response, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Stop a running service.
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancelToken.Cancel();
            return Task.CompletedTask;
        }

        public virtual void Dispose()
        {
            _cancelToken.Dispose();
        }
    }
}