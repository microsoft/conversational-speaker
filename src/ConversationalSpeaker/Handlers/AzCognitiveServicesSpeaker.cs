using System.Text.RegularExpressions;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConversationalSpeaker
{
    /// <summary>
    /// A speaker using Azure Cognitive Services text-to-speech.
    /// </summary>
    internal class AzCognitiveServicesSpeaker : IDisposable
    {
        private readonly AzureCognitiveServicesOptions _options;
        private readonly ILogger<AzCognitiveServicesSpeaker> _logger;
        private readonly SpeechSynthesizer _speechSynthesizer;

        private static readonly Regex _styleRegex = new Regex(@"(~~(.+)~~)");

        public AzCognitiveServicesSpeaker(
            IOptions<AzureCognitiveServicesOptions> options,
            ILogger<AzCognitiveServicesSpeaker> logger)
        {
            _logger = logger;
            _options = options.Value;
            _options.Validate();

            SpeechConfig speechConfig = SpeechConfig.FromSubscription(_options.Key, _options.Region);
            speechConfig.SpeechSynthesisVoiceName = _options.SpeechSynthesisVoiceName;
            _speechSynthesizer = new SpeechSynthesizer(speechConfig);

        }

        /// <summary>
        /// Speak a message.
        /// </summary>
        public async Task SpeakAsync(string message, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                // Parse speaking style, if any
                message = ExtractStyle(message, out string style);
                if (string.IsNullOrWhiteSpace(style))
                {
                    _logger.LogInformation($"Speaking (none): {message}");
                }
                else
                {
                    _logger.LogInformation($"Speaking ({style}): {message}");
                }
                
                string ssml = GenerateSsml(message, style, _options.SpeechSynthesisVoiceName);
                _logger.LogDebug(ssml);
                await _speechSynthesizer.SpeakSsmlAsync(ssml);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _speechSynthesizer.Dispose();
        }

        private string ExtractStyle(string message, out string style)
        {
            style = string.Empty;
            Match match = _styleRegex.Match(message);
            if (match.Success)
            {
                style = match.Groups[2].Value.Trim();
                message = message.Replace(match.Groups[1].Value, string.Empty).Trim();
            }
            return message;
        }

        private string GenerateSsml(string message, string style, string voiceName)
            => "<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xmlns:mstts=\"https://www.w3.org/2001/mstts\" xml:lang=\"en-US\">" + 
                $"<voice name=\"{voiceName}\">" + 
                    $"<mstts:express-as style=\"{style}\">" + 
                        $"{message}" + 
                    "</mstts:express-as>" + 
                    "</voice>" + 
                "</speak>";
    }
}