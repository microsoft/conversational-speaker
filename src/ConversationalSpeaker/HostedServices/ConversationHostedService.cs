using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConversationalSpeaker
{
    /// <summary>
    /// Hosted service to process a conversation after a new input is received.
    /// </summary>
    internal class ConversationHostedService : BaseHostedService<ConversationHostedService>
    {
        private readonly ConcurrentMessageQueue<UserInput> _userPrompts;
        private readonly ConcurrentMessageQueue<SpeakRequest> _speakRequests;
        private readonly IConversationHandler _conversationProcessor;

        /// <summary>
        /// Constructor
        /// </summary>
        public ConversationHostedService(
            IConversationHandler conversationProcessor,
            ConcurrentMessageQueue<UserInput> userPrompts,
            ConcurrentMessageQueue<SpeakRequest> speakRequests,
            ILogger<ConversationHostedService> logger,
            IHostApplicationLifetime appLifetime)
            : base(logger, appLifetime)
        {
            _userPrompts = userPrompts;
            _speakRequests = speakRequests;
            _conversationProcessor = conversationProcessor;
        }

        /// <summary>
        /// Primary logic loop.
        /// </summary>
        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await _userPrompts.Wait(cancellationToken);

                while (_userPrompts.TryDequeue(out UserInput userPrompt))
                {
                    if (!string.IsNullOrWhiteSpace(userPrompt.Message))
                    {
                        string response = await _conversationProcessor.ProcessAsync(userPrompt.Message, cancellationToken);
                        if (!string.IsNullOrWhiteSpace(response))
                        {
                            _speakRequests.Enqueue(new SpeakRequest() { Message = response });
                        }
                    }
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _userPrompts.Dispose();
            _speakRequests.Dispose();
        }
    }
}