namespace ConversationalSpeaker.Handlers.OpenAiModels
{
    internal class OpenAiCompletionResponse
    {
        public string id { get; set; }
        public string model { get; set; }
        public OpenAiCompletionResponseChoice[] choices { get; set; }
        public OpenAiCompletionResponseUsage usage { get; set; }
    }
}
