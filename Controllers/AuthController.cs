using Microsoft.AspNetCore.Mvc;

namespace automobile_backend.Controllers
{
    using System.Security.Claims;
    using automobile_backend.InterFaces.IServices;
    using automobile_backend.Models.DTOs;
    using automobile_backend.Models.Entities;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Google;
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

        // [HttpPost("login/v1")]
        // public async Task<IActionResult> Login(UserLoginDto request)
        // {
        //     // MODIFIED: Get the tuple back
        //     var (user, token) = await _authService.LoginAsync(request);

        //     if (user == null || token == null)
        //     {
        //         return Unauthorized("Invalid credentials.");
        //     }

        //     // Set token in a secure, HttpOnly cookie (unchanged)
        //     Response.Cookies.Append("jwt-token", token, new CookieOptions
        //     {
        //         HttpOnly = true,
        //         Secure = false, // TODO: Set to true in production
        //         SameSite = SameSiteMode.Strict,
        //         Expires = DateTime.UtcNow.AddDays(1)
        //     });

        //     // MODIFIED: Return user data so frontend knows the role
        //     var userDto = new
        //     {
        //         email = user.Email,
        //         firstName = user.FirstName,
        //         lastName = user.LastName,
        //         role = user.Role.ToString()
        //     };

        //     return Ok(userDto);
        // }
        [HttpPost("login/v1")]
public async Task<IActionResult> Login(
    UserLoginDto request,
    [FromServices] WebSocketNotificationService wsService // Inject WebSocket service
)
{
    // Authenticate the user
    var (user, token) = await _authService.LoginAsync(request);

    if (user == null || token == null)
        return Unauthorized("Invalid credentials.");

    // Set JWT cookie
    Response.Cookies.Append("jwt-token", token, new CookieOptions
    {
        HttpOnly = true,
        Secure = false, // Set true in production
        SameSite = SameSiteMode.Strict,
        Expires = DateTime.UtcNow.AddDays(1)
    });

    // ✅ Notify all connected admins (in JSON format)
    await wsService.NotifyAdminsAsync(
        "user_login",
        $"User logged in: {user.FirstName} {user.LastName} ({user.Email})",
        $"{user.FirstName} {user.LastName}",
        user.Email
    );

    // Return user info
    var userDto = new
    {
        email = user.Email,
        firstName = user.FirstName,
        lastName = user.LastName,
        role = user.Role.ToString()
    };

    return Ok(userDto);
}




        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {

            var redirectUrl = Url.Action(nameof(GoogleSignInHandler));

            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }


        [HttpGet("google-signin-handler")]
        public async Task<IActionResult> GoogleSignInHandler()
        {
            try
            {
                var (user, jwtToken) = await _authService.HandleGoogleLoginAsync();

                Response.Cookies.Append("jwt-token", jwtToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false, // Set to true in production (HTTPS)
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(1)
                });

                // MODIFIED: Redirect based on the user's role
                string redirectUrl = user.Role switch
                {
                    Enums.Admin => "http://localhost:3000/admin/dashboard",
                    Enums.Employee => "http://localhost:3000/employee/dashboard",
                    _ => "http://localhost:3000/customer/dashboard" // Default to customer
                };

                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                return Redirect($"http://localhost:3000/login?error={Uri.EscapeDataString(ex.Message)}");
            }
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
        public async Task<IActionResult> GetProfile()
        {
            // MODIFIED: Return full user info
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized();
            }

            var user = await _authService.GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var userDto = new
            {
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                role = user.Role.ToString()
            };

            return Ok(userDto);
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
