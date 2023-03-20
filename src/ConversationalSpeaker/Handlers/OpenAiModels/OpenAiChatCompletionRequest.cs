namespace ConversationalSpeaker.Handlers.OpenAiModels
{
    // https://beta.openai.com/docs/api-reference
    internal class OpenAIChatCompletionRequest
    {
        public string model { get; set; }
        public OpenAIChatMessage[] messages { get; set; }
        public int max_tokens { get; set; }
        public float temperature { get; set; }
        public int n { get; set; }
        public bool stream { get; set; }
        //public string stop { get; set; }
        public float presence_penalty { get; set; }
        public float frequency_penalty { get; set; }
        public string user { get; set; }
    }
}
