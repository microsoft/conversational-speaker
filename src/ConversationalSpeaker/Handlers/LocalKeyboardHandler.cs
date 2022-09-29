namespace ConversationalSpeaker
{
    /// <summary>
    /// A listener for keyboard input.
    /// </summary>
    internal class LocalKeyboardHandler : IListener
    {
        /// <inheritdoc/>
        public Task<string> ListenAsync(CancellationToken cancellationToken)
        {
            string prompt = string.Empty;
            while (string.IsNullOrWhiteSpace(prompt))
            {
                prompt = Console.ReadLine();
            }

            return Task.FromResult(prompt);
        }
    }
}
