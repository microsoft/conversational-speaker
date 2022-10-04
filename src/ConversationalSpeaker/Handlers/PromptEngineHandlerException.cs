using Microsoft.AI.PromptEngine;
using Microsoft.AI.PromptEngine.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels.ResponseModels;

namespace ConversationalSpeaker
{
    internal class PromptEngineHandlerException : Exception
    {
        public PromptEngineHandlerException()
        {
        }

        public PromptEngineHandlerException(string message) : base(message)
        {
        }

        public PromptEngineHandlerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}