namespace ConversationalSpeaker.Handlers.OpenAiModels
{
    internal class OpenAiCompletionResponseChoice
    {
        public string text { get; set; }
        public int index { get; set; }
        public string finish_reason { get; set; }
    }
}
