using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConversationalSpeaker
{
    /// <summary>
    /// A listener using Azure Cognitive Services speech-to-text
    /// </summary>
    internal class AzCognitiveServicesListener : IDisposable
    {
        private readonly ILogger _logger;
        private readonly AzureCognitiveServicesOptions _options;
        private readonly AudioConfig _audioConfig;
        private readonly SpeechRecognizer _speechRecognizer;

        /// <summary>
        /// Constructor
        /// </summary>
        public AzCognitiveServicesListener(
            IOptions<AzureCognitiveServicesOptions> options,
            ILogger<AzCognitiveServicesListener> logger)
        {
            _logger = logger;
            _options = options.Value;
            _options.Validate();

            SpeechConfig speechConfig = SpeechConfig.FromSubscription(_options.Key, _options.Region);
            speechConfig.SpeechRecognitionLanguage = _options.SpeechRecognitionLanguage;
            speechConfig.SetProperty(PropertyId.SpeechServiceResponse_PostProcessingOption, "TrueText");

            _audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            _speechRecognizer = new SpeechRecognizer(speechConfig, _audioConfig);
        }

        /// <summary>
        /// Listen to someone speaking and return the spoken text.
        /// </summary>
        public async Task<string> ListenAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested) 
            {
                SpeechRecognitionResult result = await _speechRecognizer.RecognizeOnceAsync();
                switch (result.Reason) {
                    case ResultReason.RecognizedSpeech:
                        _logger.LogInformation($"Recognized: {result.Text}");
                        return result.Text;
                    case ResultReason.Canceled:
                        _logger.LogInformation($"Speech recognizer session canceled.");
                        break;
                }
            } 
            return string.Empty;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _speechRecognizer.Dispose();
            _audioConfig.Dispose();
        }
    }
}