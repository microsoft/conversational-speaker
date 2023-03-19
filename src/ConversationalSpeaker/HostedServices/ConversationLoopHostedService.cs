using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using NetCoreAudio;

namespace ConversationalSpeaker
{
    /// <summary>
    /// A hosted service providing the primary conversation loop.
    /// </summary>
    internal class ConversationLoopHostedService : IHostedService, IDisposable
    {
        private readonly IKernel _semanticKernel;
        private readonly IDictionary<string, ISKFunction> _speechSkill;
        private readonly IDictionary<string, ISKFunction> _azOpenAISkill;
        private readonly AzCognitiveServicesWakeWordListener _wakeWordListener;
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
            IKernel semanticKernel,
            AzCognitiveServicesSpeechSkill speechSkill,
            //AzOpenAISkill openAISkill,
            OpenAISkill openAISkill,
            ILogger<ConversationLoopHostedService> logger)
        {
            _semanticKernel = semanticKernel;
            _speechSkill = _semanticKernel.ImportSkill(speechSkill);
            _azOpenAISkill = _semanticKernel.ImportSkill(openAISkill);

            _wakeWordListener = wakeWordListener;
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
                //await _semanticKernel.RunAsync("Hello! ~~friendly~~", _speechSkill["Speak"]);
                await _semanticKernel.RunAsync("Hello!", _speechSkill["Speak"]);

                // Start listening
                bool keepListening = true;
                while (keepListening && !cancellationToken.IsCancellationRequested)
                {
                    await _semanticKernel.RunAsync(
                        _speechSkill["Listen"],
                        _azOpenAISkill["Chat"],
                        _speechSkill["Speak"]);
                    
                    // TODO: User said "goodbye" - stop listening
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
        }
    }
}