using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConversationalSpeaker
{
    /// <summary>
    /// A hosted service providing the primary conversation loop.
    /// </summary>
    internal class ConversationLoopHostedService : IHostedService, IDisposable
    {
        private readonly AzCognitiveServicesWakeWordListener _wakeWordListener;
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
            AzCognitiveServicesWakeWordListener wakeWordListener,
            AzCognitiveServicesListener listener,
            AzCognitiveServicesSpeaker speaker,
            PromptEngineHandler conversationHandler,
            ILogger<ConversationLoopHostedService> logger)
        {
            _wakeWordListener = wakeWordListener;
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
            while (!cancellationToken.IsCancellationRequested)
            {
                // Wait for wake word or phrase
                _logger.LogInformation("Waiting for wake word...");
                if (!await _wakeWordListener.WaitForWakeWordAsync(cancellationToken))
                {
                    continue;
                }

                // Say hello on startup
                await _speaker.SpeakAsync("Hello!", cancellationToken);

                // Start listening
                bool keepListening = true;
                while (keepListening && !cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Listening...");
                    string userMessage = await _listener.ListenAsync(cancellationToken);

                    // User said "goodbye" - stop listening
                    if (userMessage.StartsWith("goodbye", StringComparison.OrdinalIgnoreCase))
                    {
                        await _speaker.SpeakAsync("Bye!", cancellationToken);
                        keepListening = false;
                        continue;
                    }

                    // Run what the user said through the conversation handler (i.e. AI)
                    string response = await _conversationHandler.ProcessAsync(userMessage, cancellationToken);

                    // Speak the response from the AI, if any.
                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        await _speaker.SpeakAsync(response, cancellationToken);
                    }
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

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            _cancelToken.Dispose();
            _wakeWordListener.Dispose();
            _listener.Dispose();
            _speaker.Dispose();
        }
    }
}