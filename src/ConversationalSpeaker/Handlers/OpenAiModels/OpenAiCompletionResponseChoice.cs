namespace ConversationalSpeaker.Handlers.OpenAiModels
{
    // https://beta.openai.com/docs/api-reference/completions
    internal class OpenAiCompletionResponseChoice
    {
        public string text { get; set; }
        public int index { get; set; }
        public string finish_reason { get; set; }
    }
}
