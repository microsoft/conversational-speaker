namespace ConversationalSpeaker
{
    /// <summary>
    /// A listener for keyboard input for interacting without the need to speak.
    /// </summary>
    internal class LocalKeyboardHandler
    {
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
