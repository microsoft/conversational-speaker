namespace ConversationalSpeaker
{
    /// <summary>
    /// An iterface to process a conversation.
    /// </summary>
    internal interface IConversationHandler
    {
        /// <summary>
        /// Process a conversation and received a response.
        /// </summary>
        /// <param name="prompt">Complete conversation prompt.</param>
        /// <returns>Response from the conversation.</returns>
        Task<string> ProcessAsync(string prompt, CancellationToken cancellationToken);
    }
}