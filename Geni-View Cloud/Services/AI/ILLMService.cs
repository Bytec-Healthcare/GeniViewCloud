using System.Threading;
using System.Threading.Tasks;

namespace GeniView.Cloud.Services.AI
{
    /// <summary>
    /// Abstraction over any LLM backend (Gemini, Azure OpenAI, OpenAI, etc.).
    /// Swap implementations without touching AIChatService.
    /// </summary>
    public interface ILLMService
    {
        /// <summary>
        /// Sends a system prompt + user prompt to the LLM and returns a plain-text response.
        /// </summary>
        Task<string> GenerateResponseAsync(
            string systemPrompt,
            string userPrompt,
            CancellationToken cancellationToken = default);
    }
}
