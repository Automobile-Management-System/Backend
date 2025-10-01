using Microsoft.AspNetCore.Mvc;

namespace automobile_backend.Controllers
{
    using System.Security.Claims;
    using automobile_backend.InterFaces.IServices;
    using automobile_backend.Models.DTOs;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register/v1")]
        public async Task<IActionResult> Register(UserRegisterDto request)
        {
            var user = await _authService.RegisterAsync(request);
            if (user == null)
            {
                return BadRequest("User with this email already exists.");
            }
            return Ok(new { message = "Registration successful" });
        }

        [HttpPost("login/v1")]
        public async Task<IActionResult> Login(UserLoginDto request)
        {
            var token = await _authService.LoginAsync(request);
            if (token == null)
            {
                return Unauthorized("Invalid credentials.");
            }

            // Set token in a secure, HttpOnly cookie
            Response.Cookies.Append("jwt-token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, 
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(1)
            });

            return Ok(new { message = "Login successful" });
        }

        [HttpPost("logout"), Authorize]
        public IActionResult Logout()
        {
            // Clear the cookie by setting an expired one
            Response.Cookies.Delete("jwt-token");
            return Ok(new { message = "Logout successful" });
        }

        // --- Example Protected Endpoints ---

        [HttpGet("profile"), Authorize]
        public IActionResult GetProfile()
        {
            // Example of accessing user claims from the token
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            return Ok($"Welcome {userEmail}! This is your profile.");
        }

        [HttpGet("admin-data"), Authorize(Roles = "Admin")]
        public IActionResult GetAdminData()
        {
            return Ok("This is sensitive data only for Admins.");
        }

        [HttpGet("employee-data"), Authorize(Roles = "Admin,Employee")]
        public IActionResult GetEmployeeData()
        {
            return Ok("This data is for Admins and Employees.");
        }
    }
}
