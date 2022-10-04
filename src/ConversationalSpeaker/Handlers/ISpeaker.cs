namespace ConversationalSpeaker
{
    /// <summary>
    /// Interface for speaking messages.
    /// </summary>
    internal interface ISpeaker
    {
        /// <summary>
        /// Speak a message.
        /// </summary>
        /// <param name="message">Message to speak.</param>
        Task SpeakAsync(string message, CancellationToken cancellationToken);
    }
}