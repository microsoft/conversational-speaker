using ConversationalSpeaker.Handlers.OpenAiModels;

namespace ConversationalSpeaker
{
    // https://beta.openai.com/docs/api-reference
    public class OpenAIChatCompletionResponse
    {
        public string id { get; set; }
        public string @object { get; set; }
        public int created { get; set; }
        public Choice[] choices { get; set; }
        public Usage usage { get; set; }

        public class Choice
        {
            public int index { get; set; }
            public OpenAIChatMessage message { get; set; }
            public string finish_reason { get; set; }
        }

        public class Usage
        {
            public int prompt_tokens { get; set; }
            public int completion_tokens { get; set; }
            public int total_tokens { get; set; }
        }
    }
}