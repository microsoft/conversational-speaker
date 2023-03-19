namespace ConversationalSpeaker
{
    /// <summary>
    /// Configuration for interacting with Azure OpenAI.
    /// </summary>
    public class AzureOpenAiOptions
    {
        /// <summary>
        /// API Key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Endpoint for your instance of Azure OpenAI.
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Deployement ID to target in Azure OpenAI.
        /// </summary>
        public string Deployment { get; set; }

        /// <summary>
        /// Model of deployment.
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Maximum number of tokens to use when calling OpenAI.
        /// </summary>
        public int MaxTokens { get; set; }

        /// <summary>
        /// Randomness controls (0.0 - 1.0).
        /// </summary>
        public float Temperature { get; set; }

        /// <summary>
        /// Diversity (0.0 - 1.0).
        /// </summary>
        public float TopP { get; set; }

        /// <summary>
        /// How much to penalize new tokens based on existing frequency in the text so far (0.0 - 2.0).
        /// </summary>
        public float FrequencyPenalty { get; set; }

        /// <summary>
        /// How much to penalize new tokens based on whether they appear in the text so far (0.0 - 2.0).
        /// </summary>
        public float PresencePenalty { get; set; }

        /// <summary>
        /// Validate options, throw an exception is any are invalid.
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Key))
                throw new ArgumentException("Argument is invalid.", nameof(Key));

            if (string.IsNullOrWhiteSpace(Endpoint))
                throw new ArgumentException("Argument is invalid.", nameof(Endpoint));

            if (string.IsNullOrWhiteSpace(Deployment))
                throw new ArgumentException("Argument is invalid.", nameof(Deployment));
            
            if (string.IsNullOrWhiteSpace(Model))
                throw new ArgumentException("Argument is invalid.", nameof(Model));

            if (MaxTokens < 0)
                throw new ArgumentException("Argument is invalid.", nameof(MaxTokens));

            if (Temperature < 0 || Temperature > 1)
                throw new ArgumentException("Argument is invalid.", nameof(Temperature));

            if (TopP < 0 || TopP > 1)
                throw new ArgumentException("Argument is invalid.", nameof(TopP));

            if (FrequencyPenalty < 0 || FrequencyPenalty > 2)
                throw new ArgumentException("Argument is invalid.", nameof(FrequencyPenalty));

            if (PresencePenalty < 0 || PresencePenalty > 2)
                throw new ArgumentException("Argument is invalid.", nameof(PresencePenalty));

            if (string.IsNullOrWhiteSpace(Model))
                throw new ArgumentException("Argument is invalid.", nameof(Model));
        }
    }
}
