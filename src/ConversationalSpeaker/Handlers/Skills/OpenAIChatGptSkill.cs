using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AI.Dev.OpenAI.GPT;
using ConversationalSpeaker.Handlers.OpenAiModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

namespace ConversationalSpeaker
{
    public partial class OpenAIChatGptSkill
    {
        private readonly OpenAiServiceOptions _options;
        private readonly ILogger _logger;
        private readonly List<OpenAIChatMessage> _messages;

        public const string StopListentingVariableName = "StopListening";

        public OpenAIChatGptSkill(
            IOptions<OpenAiServiceOptions> options,
            IOptions<GeneralOptions> generalOptions,
            ILogger<OpenAIChatGptSkill> logger)
        {
            _logger = logger;
            _options = options.Value;
            
            _messages = new List<OpenAIChatMessage>
            {
                new OpenAIChatMessage()
                {
                    role = "system",
                    content = generalOptions.Value.SystemPrompt
                }
            };
        }

        [SKFunction("Create a chat prompt using a prompt engine.")]
        [SKFunctionName("Chat")]
        public async Task<string> Chat(string input, SKContext context)
        {
            if (string.IsNullOrEmpty(input))
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
                content = input,
            });

            int tokenCount = GPT3Tokenizer.Encode(JsonSerializer.Serialize(_messages)).Count;
            while (tokenCount > _options.MaxTokens && _messages.Count > 2) 
            {
                _messages.RemoveRange(1, 1); // Leave the system message in place.
                tokenCount = GPT3Tokenizer.Encode(JsonSerializer.Serialize(_messages)).Count;
            }

            OpenAIChatCompletionRequest completionRequest = new OpenAIChatCompletionRequest()
            {
                model = _options.Model,
                messages = _messages.ToArray(),
                temperature = _options.Temperature,
                n = 1,
                max_tokens = _options.MaxTokens,
                presence_penalty = _options.PresencePenalty,
                frequency_penalty = _options.FrequencyPenalty,
                user = "ConversationalSpeaker"
            };

            using HttpRequestMessage request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.openai.com/v1/chat/completions"),
                Content = JsonContent.Create(completionRequest, MediaTypeHeaderValue.Parse("application/json"))
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.Key);
            
            if (!string.IsNullOrEmpty(_options.OrganizationId))
            {
                request.Headers.Add("OpenAI-Organization", _options.OrganizationId);
            }

            using HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.SendAsync(request, context.CancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"OpenAI returned an error. {response.StatusCode}: {response.ReasonPhrase}");
                _logger.LogError(await response.Content.ReadAsStringAsync(context.CancellationToken));
                return "OpenAI returned an error. Please try again.";
            }

            OpenAIChatCompletionResponse responseContent = await response.Content.ReadFromJsonAsync<OpenAIChatCompletionResponse>(cancellationToken: context.CancellationToken);

            // Add original user input
            _messages.Add(new OpenAIChatMessage()
            {
                role = "user",
                content = input
            });

            // Add AI response
            OpenAIChatCompletionResponse.Choice firstChoice = responseContent.choices.First();
            _messages.Add(responseContent.choices.First().message);

            return firstChoice.message.content;
        }
    }
}