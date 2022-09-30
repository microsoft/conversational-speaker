using Microsoft.AI.PromptEngine;
using Microsoft.AI.PromptEngine.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels.ResponseModels;

namespace ConversationalSpeaker
{
    /// <summary>
    /// A conversation processor using OpenAI and a prompt engine.
    /// </summary>
    /// <remarks>
    /// This implementation is using the Betalgo.OpenAI.GPT3 unofficial OpenAI SDK.
    /// You can find the code here: https://github.com/betalgo/openai
    /// </remarks>
    internal class PromptEngineHandler : IConversationHandler
    {
        private readonly ILogger _logger;
        private readonly OpenAiServiceOptions _openAiServiceOptions;
        private readonly Settings _promptEngineOptions;
        private readonly List<Interaction> _promptEngineInteractions = new List<Interaction>();
        private readonly IOpenAIService _openAIService;
        
        private readonly Models.Model _model;
        private readonly GenericEngine _promptEngine;

        /// <summary>
        /// Constructor
        /// </summary>
        public PromptEngineHandler(
            IOptions<OpenAiServiceOptions> openAiServiceOptions,
            IOptions<Settings> promptEngineSettings,
            IOpenAIService openAIService,
            ILogger<PromptEngineHandler> logger)
        {
            _logger = logger;
            _openAIService = openAIService;
            _openAiServiceOptions = openAiServiceOptions.Value;
            _openAiServiceOptions.Validate();
            _promptEngineOptions = promptEngineSettings.Value;

            if (!Enum.TryParse<Models.Model>(_openAiServiceOptions.Model, true, out _model))
            {
                throw new ArgumentException("Invalid model.", nameof(_openAiServiceOptions.Model));
            }

            _promptEngine = new GenericEngine(_promptEngineOptions);
        }

        /// <inheritdoc/>
        public async Task<string> ProcessAsync(string input, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            if (input.StartsWith(_promptEngineOptions.ContextResetText, StringComparison.OrdinalIgnoreCase))
            {
                _promptEngineInteractions.Clear();
                return "Okay, let's start a new conversation.";
            }

            IPrompt prompt = _promptEngine.Render(input);
            
            _logger.LogDebug($"Prompt:{Environment.NewLine}{prompt.ToString()}");

            // Send the conversation to GPT
            CompletionCreateResponse completionResult = await _openAIService.Completions.Create(
                new CompletionCreateRequest()
                {
                    Prompt = prompt.ToString(),
                    MaxTokens = _openAiServiceOptions.MaxTokens,
                    Temperature = _openAiServiceOptions.Temperature,
                    TopP = _openAiServiceOptions.TopP,
                    FrequencyPenalty = _openAiServiceOptions.FrequencyPenalty,
                    PresencePenalty = _openAiServiceOptions.PresencePenalty
                }, _model);

            if (completionResult.Successful)
            {
                string responseMessage = completionResult.Choices.FirstOrDefault().Text.Trim();

                // Add the interaction as an example for future interactions. This helps the AI keep context of the conversation.
                _promptEngineInteractions.Add(new Interaction() { Input = input, Output = responseMessage });
                _promptEngineOptions.Examples = _promptEngineInteractions.ToArray();
                
                return responseMessage;
            }
            else
            {
                string errorMessage = "OpenAI GPT3 returned an error.";
                if (completionResult.Error != null)
                {
                    errorMessage += $"{completionResult.Error.Code}: {completionResult.Error.Message}";
                }
                _logger.LogError(errorMessage);

                throw new PromptEngineHandlerException(errorMessage);
            }
        }
    }
}