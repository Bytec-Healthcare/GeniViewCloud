using GeniView.Cloud.Services.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GeniView.Cloud.Controllers.API
{
    /// <summary>
    /// POST /api/ai/chat
    /// Receives a natural-language message and returns a human-friendly AI answer.
    /// </summary>
    [ApiController]
    [Route("api/ai")]
    [Authorize]   // require login — same as all MVC pages in this app
    public class ChatApiController : ControllerBase
    {
        private readonly AIChatService _chatService;
        private readonly ILogger<ChatApiController> _logger;

        public ChatApiController(AIChatService chatService, ILogger<ChatApiController> logger)
        {
            _chatService = chatService;
            _logger      = logger;
        }

        /// <summary>
        /// POST /api/ai/chat
        /// Body: { "message": "How is battery B100 performing?" }
        /// Returns: { "answer": "Battery B100 is currently..." }
        /// </summary>
        [HttpPost("chat")]
        public async Task<IActionResult> Chat(
            [FromBody] ChatRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ChatResponse { Answer = "Invalid request." });

            var sw = Stopwatch.StartNew();
            _logger.LogInformation("AI chat — incoming message: {Message}", request.Message);

            try
            {
                var answer = await _chatService.HandleMessageAsync(
                    request.Message, cancellationToken);

                sw.Stop();
                _logger.LogInformation("AI chat — responded in {Ms}ms.", sw.ElapsedMilliseconds);

                return Ok(new ChatResponse { Answer = answer });
            }
            catch (TimeoutException ex)
            {
                _logger.LogWarning(ex, "AI chat request timed out.");
                return StatusCode(504, new ChatResponse
                {
                    Answer = "The AI service took too long to respond. Please try again."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI chat unhandled error.");
                return StatusCode(500, new ChatResponse
                {
                    Answer = "An error occurred while processing your request. Please try again."
                });
            }
        }
    }

    // ── DTOs ─────────────────────────────────────────────────────────────────

    public class ChatRequest
    {
        [Required(ErrorMessage = "Message is required.")]
        [MinLength(2, ErrorMessage = "Message is too short.")]
        [MaxLength(500, ErrorMessage = "Message must be 500 characters or fewer.")]
        public string Message { get; set; }
    }

    public class ChatResponse
    {
        public string Answer { get; set; }
    }
}
