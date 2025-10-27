using automobile_backend.InterFaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace automobile_backend.Controllers
{
    // Simple DTO for user input
    public class ChatRequestDto
    {
        public string Question { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;

        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        /// <summary>
        /// Sends a natural language question to the AI chatbot to get a data-driven answer.
        /// </summary>
        [HttpPost("ask")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Ask([FromBody] ChatRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                return BadRequest("Question cannot be empty.");
            }

            try
            {
                var answer = await _chatbotService.AnswerQuestionAsync(request.Question);

                if (string.IsNullOrEmpty(answer))
                {
                    return Ok(new { Answer = "Sorry, I was unable to process your request. Check your query structure or the backend logs." });
                }

                return Ok(new { Answer = answer });

            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Chatbot Error: {ex.Message}");
                return StatusCode(500, new { Answer = "An unexpected error occurred while processing your request." });
            }
        }
    }
}