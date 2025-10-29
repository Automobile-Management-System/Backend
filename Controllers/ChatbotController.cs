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
    [AllowAnonymous] // <-- CHANGE: Allow unauthenticated requests initially
    public class ChatbotController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;

        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        /// <summary>
        /// Sends a natural language question to the AI chatbot.
        /// Provides personalized responses for authenticated users and general info for guests.
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

            // --- Default values for Guest users ---
            int userId = -1; // Use -1 or 0 to indicate guest
            string userRole = "Guest";
            string? userName = null; // Nullable for guest

            // --- Check if the user IS Authenticated ---
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var roleClaim = User.FindFirstValue(ClaimTypes.Role);
                var nameClaim = User.FindFirstValue(ClaimTypes.Name); // Get user's name

                // Try to parse UserId, role must exist for authenticated user
                if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out var parsedUserId) && !string.IsNullOrEmpty(roleClaim))
                {
                    userId = parsedUserId;
                    userRole = roleClaim;
                    userName = nameClaim; // Assign the name if found
                }
                else
                {
                    // Handle case where token is present but claims are missing/invalid
                    // You might want to log this or return a specific error
                    // For now, treat as Guest for safety
                    Console.WriteLine("Warning: Authenticated user has invalid/missing claims.");
                    userRole = "Guest";
                    userId = -1;
                    userName = null;
                }
            }
            // ----------------------------------------

            try
            {
                // Pass user identity (including name) to the service
                var answer = await _chatbotService.AnswerQuestionAsync(request.Question, userId, userRole, userName); // Pass userName

                if (string.IsNullOrEmpty(answer))
                {
                    return Ok(new { Answer = "Sorry, I couldn't process that request right now." });
                }

                return Ok(new { Answer = answer });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chatbot Error: {ex.Message}");
                return StatusCode(500, new { Answer = "An unexpected error occurred." });
            }
        }
    }
}