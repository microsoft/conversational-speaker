using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConversationalSpeaker
{
    /// <summary>
    /// A listener using Azure Cognitive Services Speech-To-Text
    /// </summary>
    internal class AzCognitiveServicesListener : IListener
    {
        private readonly ILogger _logger;
        private readonly AzureCognitiveServicesOptions _options;

        /// <summary>
        /// Constructor
        /// </summary>
        public AzCognitiveServicesListener(
            IOptions<AzureCognitiveServicesOptions> options,
            ILogger<ListenerHostedService> logger)
        {
            _logger = logger;
            _options = options.Value;
            _options.Validate();
        }

        /// <inheritdoc/>
        public async Task<string> ListenAsync(CancellationToken cancellationToken)
        {
            string result = "";
            TaskCompletionSource recognitionEnd = new TaskCompletionSource();

            SpeechConfig speechConfig = SpeechConfig.FromSubscription(_options.Key, _options.Region);
            speechConfig.SpeechRecognitionLanguage = _options.SpeechRecognitionLanguage;

            using AudioConfig audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            using SpeechRecognizer speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

            speechConfig.SetProperty(PropertyId.SpeechServiceResponse_PostProcessingOption, "2");

            speechRecognizer.Recognized += (object sender, SpeechRecognitionEventArgs e) =>
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

            speechRecognizer.Canceled += (object sender, SpeechRecognitionCanceledEventArgs e) =>
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

            speechRecognizer.SessionStopped += (object sender, SessionEventArgs e) =>
            {
                _logger.LogInformation($"Stopped listening.");
            };

            await speechRecognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
            
            Task.WaitAll(new[] { recognitionEnd.Task });
            
            await speechRecognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            
            return result;
        }
    }
}
