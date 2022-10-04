using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConversationalSpeaker
{
    /// <summary>
    /// 
    /// </summary>
    internal class AzCognitiveServicesSpeaker : ISpeaker
    {
        private readonly AzureCognitiveServicesOptions _options;
        private readonly ILogger<AzCognitiveServicesSpeaker> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public AzCognitiveServicesSpeaker(
            IOptions<AzureCognitiveServicesOptions> options,
            ILogger<AzCognitiveServicesSpeaker> logger)
        {
            _logger = logger;
            _options = options.Value;
            _options.Validate();
        }

        /// <inheritdoc/>
        public async Task SpeakAsync(string message, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Speaking: {message}");

            SpeechConfig speechConfig = SpeechConfig.FromSubscription(_options.Key, _options.Region);
            speechConfig.SpeechSynthesisVoiceName = _options.SpeechSynthesisVoiceName;

            if (!string.IsNullOrWhiteSpace(message))
            {
                using (SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer(speechConfig))
                {
                    await speechSynthesizer.SpeakTextAsync(message);
                }
            }
        }
    }
}