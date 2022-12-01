using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetCoreAudio;
using System.Reflection;

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

        // Notification sound support
        private readonly string _notificationSoundFilePath;
        private readonly Player _player;

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

            _notificationSoundFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Handlers", "bing.mp3");
            _player = new Player();
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
            // Play a notification to let the user know the app has started.
            await _player.Play(_notificationSoundFilePath);

            while (!cancellationToken.IsCancellationRequested)
            {
                // Wait for wake word or phrase
                if (!await _wakeWordListener.WaitForWakeWordAsync(cancellationToken))
                {
                    continue;
                }

                await _player.Play(_notificationSoundFilePath);

                // Say hello on startup
                await _speaker.SpeakAsync("Hello! ~~friendly~~", cancellationToken);

                // Start listening
                bool keepListening = true;
                while (keepListening && !cancellationToken.IsCancellationRequested)
                {
                    string userMessage = await _listener.ListenAsync(cancellationToken);

                    // User said "goodbye" - stop listening
                    if (userMessage.StartsWith("goodbye", StringComparison.OrdinalIgnoreCase))
                    {
                        await _speaker.SpeakAsync("Bye!", cancellationToken);
                        await _player.Play(_notificationSoundFilePath);
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