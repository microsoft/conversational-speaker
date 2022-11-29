using ConversationalSpeaker.Handlers.OpenAiModels;
using Microsoft.AI.PromptEngine;
using Microsoft.AI.PromptEngine.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ConversationalSpeaker
{
    /// <summary>
    /// A conversation processor using OpenAI and a prompt engine.
    /// </summary>
    /// <remarks>
    /// This implementation is using the Betalgo.OpenAI.GPT3 unofficial OpenAI SDK.
    /// You can find the code here: https://github.com/betalgo/openai
    /// </remarks>
    internal class PromptEngineHandler
    {
        private readonly OpenAiServiceOptions _openAiServiceOptions;
        private readonly Settings _promptEngineOptions;
        private readonly ILogger _logger;


        private readonly List<Interaction> _promptEngineInteractions = new List<Interaction>();

        private readonly GenericEngine _promptEngine;

        /// <summary>
        /// Constructor
        /// </summary>
        public PromptEngineHandler(
            IOptions<OpenAiServiceOptions> openAiServiceOptions,
            IOptions<Settings> promptEngineSettings,
            ILogger<PromptEngineHandler> logger)
        {
            _logger = logger;
            _openAiServiceOptions = openAiServiceOptions.Value;
            _openAiServiceOptions.Validate();
            _promptEngineOptions = promptEngineSettings.Value;

            _promptEngine = new GenericEngine(_promptEngineOptions);
        }

        /// <summary>
        /// Take input from the user, process with the prompt engine, send the rendered prompt to GPT-3, and return the response.
        /// </summary>
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

            OpenAiCompletionRequest completionRequest = new OpenAiCompletionRequest()
            {
                prompt = prompt.ToString(),
                model = _openAiServiceOptions.Model,
                max_tokens = _openAiServiceOptions.MaxTokens,
                temperature = _openAiServiceOptions.Temperature,
                top_p = _openAiServiceOptions.TopP,
                frequency_penalty = _openAiServiceOptions.FrequencyPenalty,
                presence_penalty = _openAiServiceOptions.PresencePenalty,
                stream = false,
                stop = "\n",
                n = 1
            };
            
            using HttpRequestMessage request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.openai.com/v1/completions"),
                Content = JsonContent.Create(completionRequest, MediaTypeHeaderValue.Parse("application/json"))
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _openAiServiceOptions.Key);

            using HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"OpenAI GPT3 returned an error. {response.StatusCode}: {response.ReasonPhrase}");
                _logger.LogError(await response.Content.ReadAsStringAsync());
                return "OpenAI GPT3 returned an error. Please try again.";
            }

            OpenAiCompletionResponse responseContent = await response.Content.ReadFromJsonAsync<OpenAiCompletionResponse>();
            string responseMessage = responseContent.choices.FirstOrDefault()?.text?.Trim();

            if (!string.IsNullOrWhiteSpace(responseMessage))
            {
                // Add the interaction as an example for future interactions. This helps the AI keep context of the conversation.
                _promptEngineInteractions.Add(new Interaction() { Input = input, Output = responseMessage });
                _promptEngineOptions.Examples = _promptEngineInteractions.ToArray();
            }
            
            return responseMessage;
        }
    }
}
  