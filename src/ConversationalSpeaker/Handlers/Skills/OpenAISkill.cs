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
    // TODO - remove this class and use Semantic Kernel's Semantic Functions instead.
    public partial class OpenAISkill
    {
        private readonly OpenAiServiceOptions _openAiServiceOptions;
        private readonly ILogger _logger;
        private readonly List<OpenAIChatMessage> _messages;

        public OpenAISkill(
            IOptions<OpenAiServiceOptions> openAiServiceOptions,
            IOptions<GeneralOptions> generalOptions,
            ILogger<OpenAISkill> logger)
        {
            _logger = logger;
            _openAiServiceOptions = openAiServiceOptions.Value;
            
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

            _messages.Add(new OpenAIChatMessage()
            {
                role = "user",
                content = input,
            });

            List<int> tokens = GPT3Tokenizer.Encode(JsonSerializer.Serialize(_messages));
            int tokenCount = tokens.Count;
            // TODO - Make sure we are still under token limit, otherwise remove context until we are.

            OpenAIChatCompletionRequest completionRequest = new OpenAIChatCompletionRequest()
            {
                model = _openAiServiceOptions.Model,
                messages = _messages.ToArray(),
                temperature = _openAiServiceOptions.Temperature,
                top_p = _openAiServiceOptions.TopP,
                n = 1,
                stream = false,
                stop = "\n",
                max_tokens = _openAiServiceOptions.MaxTokens,
                presence_penalty = _openAiServiceOptions.PresencePenalty,
                frequency_penalty = _openAiServiceOptions.FrequencyPenalty,
                user = "ConversationalSpeaker"
            };

            using HttpRequestMessage request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.openai.com/v1/chat/completions"),
                Content = JsonContent.Create(completionRequest, MediaTypeHeaderValue.Parse("application/json"))
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _openAiServiceOptions.Key);
            
            if (!string.IsNullOrEmpty(_openAiServiceOptions.OrganizationId))
            {
                request.Headers.Add("OpenAI-Organization", _openAiServiceOptions.OrganizationId);
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

            OpenAIChatCompletionResponse.Choice firstChoice = responseContent.choices.First();
            _messages.Add(responseContent.choices.First().message);

            return firstChoice.message.content;
        }
    }
}