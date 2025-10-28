using automobile_backend.InterFaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims; // Required for getting user claims

namespace automobile_backend.Controllers
{
    // Simple DTO for user input
    public class ChatRequestDto
    {
        public string Question { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // <-- IMPORTANT: Secures the entire controller
    public class ChatbotController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;

        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        /// <summary>
        /// Sends a natural language question to the AI chatbot.
        /// Access is restricted based on the user's role (Admin, Employee, Customer).
        /// </summary>
        [HttpPost("ask")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)] // Unauthorized
        public async Task<IActionResult> Ask([FromBody] ChatRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                return BadRequest("Question cannot be empty.");
            }

            // --- NEW: Get User ID and Role from the JWT Token ---
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userIdString) || string.IsNullOrEmpty(userRole) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Invalid user token. Unable to identify user.");
            }
            // ---------------------------------------------------

            try
            {
                // Pass the user's identity to the service for data scoping
                var answer = await _chatbotService.AnswerQuestionAsync(request.Question, userId, userRole);

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