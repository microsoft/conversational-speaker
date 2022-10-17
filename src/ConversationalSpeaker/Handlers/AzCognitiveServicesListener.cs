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

            _audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            _speechRecognizer = new SpeechRecognizer(speechConfig, _audioConfig);

            speechConfig.SetProperty(PropertyId.SpeechServiceResponse_PostProcessingOption, "2");
        }

        /// <summary>
        /// Listen to someone speaking and return the spoken text.
        /// </summary>
        public async Task<string> ListenAsync(CancellationToken cancellationToken)
        {
            string result = "";
            TaskCompletionSource recognitionEnd = new TaskCompletionSource();

            _speechRecognizer.Recognized += (object sender, SpeechRecognitionEventArgs e) =>
            {
                if (ResultReason.RecognizedSpeech == e.Result.Reason &&
                    !string.IsNullOrWhiteSpace(e.Result.Text))
                {
                    _logger.LogInformation($"Recognized: {e.Result.Text}");
                    result = e.Result.Text;
                    recognitionEnd.SetResult();
                }
                else if (ResultReason.NoMatch == e.Result.Reason)
                {
                    _logger.LogWarning($"Speech could not be recognized.");
                }
            };

            _speechRecognizer.Canceled += (object sender, SpeechRecognitionCanceledEventArgs e) =>
            {
                if (CancellationReason.Error == e.Reason)
                {
                    _logger.LogError($"{$"Error code: {(int)e.ErrorCode}, Error details: {e.ErrorDetails}"}");
                }
                else
                {
                    _logger.LogInformation($"Speech recognizer session canceled.");
                }
                recognitionEnd.SetCanceled();
            };

            _speechRecognizer.SessionStopped += (object sender, SessionEventArgs e) =>
            {
                _logger.LogInformation($"Stopped listening.");
            };

            await _speechRecognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
            
            Task.WaitAll(new[] { recognitionEnd.Task });
            
            await _speechRecognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            
            return result;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _speechRecognizer.Dispose();
            _audioConfig.Dispose();
        }
    }
}
