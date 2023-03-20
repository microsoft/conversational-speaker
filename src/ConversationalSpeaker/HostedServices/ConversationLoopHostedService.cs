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
        private readonly IDictionary<string, ISKFunction> _openAISkill;
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
            OpenAIChatGptSkill openAISkill,
            ILogger<ConversationLoopHostedService> logger)
        {
            _semanticKernel = semanticKernel;
            _speechSkill = _semanticKernel.ImportSkill(speechSkill);
            _openAISkill = _semanticKernel.ImportSkill(openAISkill);

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
            while (!cancellationToken.IsCancellationRequested)
            {
                // Play a notification to let the user know we have started listening for the wake phrase.
                await _player.Play(_notificationSoundFilePath);

                // Wait for wake word or phrase
                if (!await _wakeWordListener.WaitForWakeWordAsync(cancellationToken))
                {
                    continue;
                }

                await _player.Play(_notificationSoundFilePath);

                // Say hello on startup
                await _semanticKernel.RunAsync("Hello!", _speechSkill["Speak"]);

                // Start listening
                bool keepListening = true;
                while (keepListening && !cancellationToken.IsCancellationRequested)
                {
                    SKContext context = await _semanticKernel.RunAsync(
                        _speechSkill["Listen"],
                        _openAISkill["Chat"],
                        _speechSkill["Speak"]) ;

                    if (context.Variables.ContainsKey(AzOpenAIChatGptSkill.StopListentingVariableName) &&
                        !string.IsNullOrWhiteSpace(context[AzOpenAIChatGptSkill.StopListentingVariableName]))
                    {
                        break;
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
        }
    }
}