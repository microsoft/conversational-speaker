using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace ConversationalSpeaker
{
    /// <summary>
    /// A wake word listener using Azure Cognitive Services keyword recognition.
    /// </summary>
    internal class AzCognitiveServicesWakeWordListener : IDisposable
    {
        private readonly ILogger _logger;
        private readonly AudioConfig _audioConfig;
        private readonly KeywordRecognizer _keywordRecognizer;
        private readonly KeywordRecognitionModel _keywordModel;

        public AzCognitiveServicesWakeWordListener(
                ILogger<AzCognitiveServicesWakeWordListener> logger)
        {
            _logger = logger;

            string keywordModelPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "handlers/azcog-hey-janet-default.table");
            _keywordModel = KeywordRecognitionModel.FromFile(keywordModelPath);
            _audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            _keywordRecognizer = new KeywordRecognizer(_audioConfig);
        }

        /// <summary>
        /// Wait for the wake word or phrase to be detected before returning.
        /// </summary>
        public async Task<bool> WaitForWakeWordAsync(CancellationToken cancellationToken)
        {
            KeywordRecognitionResult result;
            do
            {
                result = await _keywordRecognizer.RecognizeOnceAsync(_keywordModel);
                _logger.LogInformation($"{result.Reason}");
            } while (result.Reason != ResultReason.RecognizedKeyword);
            return true;
        }

        public void Dispose()
        {
            _audioConfig.Dispose();
            _keywordRecognizer.Dispose();
        }
    }
}