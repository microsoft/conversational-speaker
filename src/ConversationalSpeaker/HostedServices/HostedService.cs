using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
using NetCoreAudio;

namespace ConversationalSpeaker
{
    /// <summary>
    /// A hosted service providing the primary conversation loop for Semantic Kernel with OpenAI ChatGPT.
    /// </summary>
    internal class HostedService : IHostedService, IDisposable
    {
        private readonly ILogger<HostedService> _logger;

        // Semantic Kernel chat support
        private readonly IKernel _semanticKernel;
        private readonly IDictionary<string, ISKFunction> _speechSkill;
        private readonly AzCognitiveServicesWakeWordListener _wakeWordListener;
        private readonly IChatCompletion _chatCompletion;
        private readonly OpenAIChatHistory _chatHistory;
        private readonly ChatRequestSettings _chatRequestSettings;

        private Task _executeTask;
        private readonly CancellationTokenSource _cancelToken = new();

        // Notification sound support
        private readonly string _notificationSoundFilePath;
        private readonly Player _player;

        /// <summary>
        /// Constructor
        /// </summary>
        public HostedService(
            AzCognitiveServicesWakeWordListener wakeWordListener,
            IKernel semanticKernel,
            AzCognitiveServicesSpeechSkill speechSkill,
            IOptions<OpenAiServiceOptions> openAIOptions,
            IOptions<AzureOpenAiServiceOptions> azureOpenAIOptions,
            IOptions<GeneralOptions> generalOptions,
            ILogger<HostedService> logger)
        {
            _logger = logger;
            
            _semanticKernel = semanticKernel;

            //
            // To switch from using OpenAI to Azure OpenAI, comment out the "OpenAI" section below
            // and uncomment the subsequent "Azure OpenAI" section. Make sure you have updated
            // the `/src/configuration.json` with your Azure OpenAI settings as well.
            // 

            // OpenAI BEGIN
            _chatRequestSettings = new ChatRequestSettings()
            {
                MaxTokens = openAIOptions.Value.MaxTokens,
                Temperature = openAIOptions.Value.Temperature,
                FrequencyPenalty = openAIOptions.Value.FrequencyPenalty,
                PresencePenalty = openAIOptions.Value.PresencePenalty,
                TopP = openAIOptions.Value.TopP,
                StopSequences = new string[] { "\n\n" }
            };
            _semanticKernel.Config.AddOpenAIChatCompletionService(
                openAIOptions.Value.Model, openAIOptions.Value.Key, alsoAsTextCompletion: true, logger: _logger);
            // OpenAI END

            // Azure OpenAI BEGIN
            //_chatRequestSettings = new ChatRequestSettings()
            //{
            //    MaxTokens = azureOpenAIOptions.Value.MaxTokens,
            //    Temperature = azureOpenAIOptions.Value.Temperature,
            //    FrequencyPenalty = azureOpenAIOptions.Value.FrequencyPenalty,
            //    PresencePenalty = azureOpenAIOptions.Value.PresencePenalty,
            //    TopP = azureOpenAIOptions.Value.TopP,
            //    StopSequences = new string[] { "\n\n" }
            //};
            //_semanticKernel.Config.AddAzureChatCompletionService(
            //    azureOpenAIOptions.Value.Deployment, azureOpenAIOptions.Value.Endpoint, azureOpenAIOptions.Value.Key, alsoAsTextCompletion: true, logger: _logger);
            // Azure OpenAI END

            _wakeWordListener = wakeWordListener;

            _chatCompletion = _semanticKernel.GetService<IChatCompletion>();
            _chatHistory = (OpenAIChatHistory)_chatCompletion.CreateNewChat(generalOptions.Value.SystemPrompt);

            _speechSkill = _semanticKernel.ImportSkill(speechSkill);

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
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Listen to the user
                    SKContext context = await _semanticKernel.RunAsync(_speechSkill["Listen"]);
                    string userSpoke = context.Result;

                    // Get a reply from the AI and add it to the chat history.
                    string reply = string.Empty;
                    try
                    {
                        _chatHistory.AddUserMessage(userSpoke);
                        reply = await _chatCompletion.GenerateMessageAsync(_chatHistory, _chatRequestSettings);

                        // Add the interaction to the chat history.
                        _chatHistory.AddAssistantMessage(reply);
                    }
                    catch (AIException aiex)
                    {
                        _logger.LogError($"OpenAI returned an error. {aiex.ErrorCode}: {aiex.Message}");
                        reply = "OpenAI returned an error. Please try again.";
                    }

                    // Speak the AI's reply
                    await _semanticKernel.RunAsync(reply, _speechSkill["Speak"]);

                    // If the user said "Goodbye" - stop listening and wait for the wake work again.
                    if (userSpoke.StartsWith("goodbye", StringComparison.InvariantCultureIgnoreCase))
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