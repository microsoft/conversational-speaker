namespace ConversationalSpeaker
{
    /// <summary>
    /// Interface for listening to input.
    /// </summary>
    internal interface IListener
    {
        /// <summary>
        /// Listen for input.
        /// </summary>
        /// <returns>Text input received.</returns>
        Task<string> ListenAsync(CancellationToken cancellationToken);
    }
}