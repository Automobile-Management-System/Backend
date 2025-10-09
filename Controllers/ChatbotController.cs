using automobile_backend.InterFaces.IServices;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace automobile_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;

        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] string message)
        {
            var response = await _chatbotService.GetResponseAsync(message);
            return Ok(response);
        }
    }
}
