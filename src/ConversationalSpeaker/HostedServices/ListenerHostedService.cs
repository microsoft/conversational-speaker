using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConversationalSpeaker
{
    /// <summary>
    /// Hosted service to handle listening to a user.
    /// </summary>
    internal class ListenerHostedService : BaseHostedService<ListenerHostedService>
    {
        private readonly AzureCognitiveServicesOptions _options;
        private readonly ConcurrentMessageQueue<UserInput> _userPrompts;
        private readonly ConcurrentMessageQueue<SpeakRequest> _speakRequests;
        private readonly IListener _listener;
        private readonly ReadyToListenSignal _readyToListenSignal;

        /// <summary>
        /// Constructor
        /// </summary>
        public ListenerHostedService(
            IListener listener,
            ReadyToListenSignal readyToListenSignal,
            IOptions<AzureCognitiveServicesOptions> options,
            ConcurrentMessageQueue<UserInput> userPrompts,
            ConcurrentMessageQueue<SpeakRequest> speakRequests,
        ILogger<ListenerHostedService> logger,
            IHostApplicationLifetime appLifetime)
            : base(logger, appLifetime)
        {
            _userPrompts = userPrompts;
            _speakRequests = speakRequests;
            _options = options.Value;
            _options.Validate();
            _listener = listener;
            _readyToListenSignal = readyToListenSignal;
        }

        /// <summary>
        /// Primary service logic loop.
        /// </summary>
        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _speakRequests.Enqueue(new SpeakRequest() { Message = "Hello!" });
            await _readyToListenSignal.WaitForReady(cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                await _readyToListenSignal.WaitForReady(cancellationToken);
                _logger.LogInformation("Listening...");
                string message = await _listener.ListenAsync(cancellationToken);
                
                _userPrompts.Enqueue(new UserInput() { Message = message });
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _userPrompts.Dispose();
            _readyToListenSignal.Dispose();
        }
    }
}
