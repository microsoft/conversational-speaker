using System.Text;
using AI.Dev.OpenAI.GPT;
using System.Text.Json;
using Azure;
using Azure.AI.OpenAI;
using ConversationalSpeaker.Handlers.OpenAiModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

namespace ConversationalSpeaker
{
    public class AzOpenAISkill
    {
        private readonly ILogger _logger;
        private readonly OpenAIClient _client;
        private readonly AzureOpenAiOptions _options;
        private readonly GeneralOptions _generalOptions;

        private const string _prompt = "\n<|im_start|>assistant\n";
        private readonly string _stop = "<|im_end|>";
        private readonly List<OpenAIChatMessage> _messages;

        public const string StopListentingVariableName = "AzCognitiveServicesSpeechSkill:StopListening";

        public AzOpenAISkill(
            IOptions<AzureOpenAiOptions> options,
            IOptions<GeneralOptions> generalOptions,
            ILogger<AzOpenAISkill> logger)
        {
            _logger = logger;
            _options = options.Value;
            _generalOptions = generalOptions.Value;

            _messages = new List<OpenAIChatMessage>(); 
            _client = new OpenAIClient(new Uri(_options.Endpoint), new AzureKeyCredential(_options.Key));
        }

        [SKFunction("Send the next user input to a ChatML-compatible model and get a response.")]
        [SKFunctionName("Chat")]
        public async Task<string> Chat(string input, SKContext context)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(input) &&
               input.StartsWith("goodbye", StringComparison.InvariantCultureIgnoreCase))
            {
                context[StopListentingVariableName] = true.ToString();
            }
            else
            {
                context[StopListentingVariableName] = string.Empty;
            }

            _messages.Add(new OpenAIChatMessage()
            {
                role = "user",
                content = input
            });

            string fullPrompt = ToChatML(_generalOptions.SystemPrompt, _messages) + _prompt;

            int tokenCount = GPT3Tokenizer.Encode(JsonSerializer.Serialize(fullPrompt)).Count;
            while (tokenCount > _options.MaxTokens)
            {
                _messages.RemoveRange(0, 1);
                tokenCount = GPT3Tokenizer.Encode(JsonSerializer.Serialize(fullPrompt)).Count;
            }

            CompletionsOptions co = new CompletionsOptions()
            {
                MaxTokens = _options.MaxTokens,
                Temperature = _options.Temperature,
                PresencePenalty = _options.PresencePenalty,
                FrequencyPenalty = _options.FrequencyPenalty,
                Model = _options.Model
            };

            co.Prompt.Add(ToChatML(_generalOptions.SystemPrompt, _messages) + _prompt);
            co.Stop.Add(_stop);

            Response<Completions> completions = await _client.GetCompletionsAsync(_options.Deployment, co, context.CancellationToken);
            string response = completions.Value.Choices.First().Text;
            _messages.Add(new OpenAIChatMessage()
            {
                role = "assistant",
                content = response
            });
            return response;
        }

        public string ToChatML(string systemMessage, IList<OpenAIChatMessage> messages)
        {
            StringBuilder builder = new StringBuilder();
            
            builder.Append($"<|im_start|>system\n{systemMessage}\n<|im_end|>");
            
            foreach (OpenAIChatMessage message in messages)
            {
                builder.Append($"\n<|im_start|>{message.role}\n{message.content}\n<|im_end|>");
            }

            return builder.ToString();
        }
    }
}
