namespace ConversationalSpeaker.Handlers.OpenAiModels
{
    // https://beta.openai.com/docs/api-reference/completions
    internal class OpenAiCompletionResponseUsage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
    }
}
