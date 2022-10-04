using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConversationalSpeaker
{
    /// <summary>
    /// Hosted service to handle speaking responses.
    /// </summary>
    internal class SpeakerHostedService : BaseHostedService<SpeakerHostedService>
    {
        private readonly AzureCognitiveServicesOptions _options;
        private readonly ConcurrentMessageQueue<SpeakRequest> _speakRequests;
        private readonly ISpeaker _speaker;
        private readonly ReadyToListenSignal _readyToListenSignal;

        /// <summary>
        /// Constructor
        /// </summary>
        public SpeakerHostedService(
            ISpeaker speaker,
            ReadyToListenSignal readyToListenSignal,
            IOptions<AzureCognitiveServicesOptions> options,
            ConcurrentMessageQueue<SpeakRequest> speakRequests,
            ILogger<SpeakerHostedService> logger,
            IHostApplicationLifetime appLifetime)
            : base(logger, appLifetime)
        {
            _speakRequests = speakRequests;
            _options = options.Value;
            _options.Validate();

            _speaker = speaker;
            _readyToListenSignal = readyToListenSignal;
        }

        /// <summary>
        /// Primary service logic loop.
        /// </summary>
        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _readyToListenSignal.SetReady(); // Make sure we are listening at the start.

            SpeechConfig speechConfig = SpeechConfig.FromSubscription(
                _options.Key,
                _options.Region);

            speechConfig.SpeechSynthesisVoiceName = _options.SpeechSynthesisVoiceName;

            while (true)
            {
                await _speakRequests.Wait(cancellationToken);

                while (_speakRequests.TryDequeue(out SpeakRequest item))
                {
                    if (!string.IsNullOrWhiteSpace(item.Message))
                    {
                        string message = item.Message.Trim();
                        
                        _logger.LogInformation($"Speaking: {message}");
                        await _speaker.SpeakAsync(message, cancellationToken);
                        
                        _readyToListenSignal.SetReady();
                    }
                }
            }

        }

        public override void Dispose()
        {
            base.Dispose();
            _speakRequests.Dispose();
            _readyToListenSignal.Dispose();
        }
    }
}