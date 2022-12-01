namespace ConversationalSpeaker.Handlers.OpenAiModels
{
    // https://beta.openai.com/docs/api-reference/completions
    internal class OpenAiCompletionResponse
    {
        public string id { get; set; }
        public string model { get; set; }
        public OpenAiCompletionResponseChoice[] choices { get; set; }
        public OpenAiCompletionResponseUsage usage { get; set; }
    }
}
