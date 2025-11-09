//using Microsoft.AspNetCore.Mvc;

//namespace automobile_backend.Controllers
//{
//    using System.Security.Claims;
//    using automobile_backend.InterFaces.IServices;
//    using automobile_backend.Models.DTOs;
//    using automobile_backend.Models.Entities;
//    using Microsoft.AspNetCore.Authentication;
//    using Microsoft.AspNetCore.Authentication.Google;
//    using Microsoft.AspNetCore.Authorization;
//    using Microsoft.AspNetCore.Mvc;

//    [Route("api/[controller]")]
//    [ApiController]
//    public class AuthController : ControllerBase
//    {
//        private readonly IAuthService _authService;

//        public AuthController(IAuthService authService)
//        {
//            _authService = authService;
//        }

//        [HttpPost("register/v1")]
//        public async Task<IActionResult> Register(UserRegisterDto request)
//        {
//            var user = await _authService.RegisterAsync(request);
//            if (user == null)
//            {
//                return BadRequest("User with this email already exists.");
//            }
//            return Ok(new { message = "Registration successful" });
//        }

//        [HttpPost("login/v1")]
//        public async Task<IActionResult> Login(UserLoginDto request)
//        {
//            // MODIFIED: Get the tuple back
//            var (user, token) = await _authService.LoginAsync(request);

//            if (user == null || token == null)
//            {
//                return Unauthorized("Invalid credentials.");
//            }

//            // Set token in a secure, HttpOnly cookie (unchanged)
//            Response.Cookies.Append("jwt-token", token, new CookieOptions
//            {
//                HttpOnly = true,
//                Secure = false, // TODO: Set to true in production
//                SameSite = SameSiteMode.Strict,
//                Expires = DateTime.UtcNow.AddDays(1)
//            });

//            // MODIFIED: Return user data so frontend knows the role
//            var userDto = new
//            {
//                email = user.Email,
//                firstName = user.FirstName,
//                lastName = user.LastName,
//                role = user.Role.ToString()
//            };

//            return Ok(userDto);
//        }

//        [HttpGet("google-login")]
//        public IActionResult GoogleLogin()
//        {

//            var redirectUrl = Url.Action(nameof(GoogleSignInHandler));

//            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
//            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
//        }


//        [HttpGet("google-signin-handler")]
//        public async Task<IActionResult> GoogleSignInHandler()
//        {
//            try
//            {
//                var (user, jwtToken) = await _authService.HandleGoogleLoginAsync();

//                Response.Cookies.Append("jwt-token", jwtToken, new CookieOptions
//                {
//                    HttpOnly = true,
//                    Secure = false, // Set to true in production (HTTPS)
//                    SameSite = SameSiteMode.Strict,
//                    Expires = DateTime.UtcNow.AddDays(1)
//                });

//                // MODIFIED: Redirect based on the user's role
//                string redirectUrl = user.Role switch
//                {
//                    Enums.Admin => "http://localhost:3000/admin/dashboard",
//                    Enums.Employee => "http://localhost:3000/employee/dashboard",
//                    _ => "http://localhost:3000/customer/dashboard" // Default to customer
//                };

//                return Redirect(redirectUrl);
//            }
//            catch (Exception ex)
//            {
//                return Redirect($"http://localhost:3000/login?error={Uri.EscapeDataString(ex.Message)}");
//            }
//        }


//        [HttpPost("logout"), Authorize]
//        public IActionResult Logout()
//        {
//            // Clear the cookie by setting an expired one
//            Response.Cookies.Delete("jwt-token");
//            return Ok(new { message = "Logout successful" });
//        }

//        // --- Example Protected Endpoints ---

//        [HttpGet("profile"), Authorize]
//        public async Task<IActionResult> GetProfile()
//        {
//            // MODIFIED: Return full user info including employee/customer ID
//            var userEmail = User.FindFirstValue(ClaimTypes.Email);
//            if (string.IsNullOrEmpty(userEmail))
//            {
//                return Unauthorized();
//            }

//            var user = await _authService.GetUserByEmailAsync(userEmail);
//            if (user == null)
//            {
//                return NotFound("User not found.");
//            }

//            // Base user data
//            var userDto = new
//            {
//                id = user.UserId,
//                email = user.Email,
//                firstName = user.FirstName,
//                lastName = user.LastName,
//                role = user.Role.ToString()
//            };

//            // For employees and customers, add the specific ID (which is the same as UserId in this system)
//            if (user.Role == Enums.Employee)
//            {
//                var employeeProfile = new
//                {
//                    userDto.id,
//                    employeeId = user.UserId, // Employee ID is the same as User ID
//                    userDto.email,
//                    userDto.firstName,
//                    userDto.lastName,
//                    userDto.role
//                };
//                return Ok(employeeProfile);
//            }
//            else if (user.Role == Enums.Customer)
//            {
//                var customerProfile = new
//                {
//                    userDto.id,
//                    customerId = user.UserId, // Customer ID is the same as User ID
//                    userDto.email,
//                    userDto.firstName,
//                    userDto.lastName,
//                    userDto.role
//                };
//                return Ok(customerProfile);
//            }

//            // For other roles (Admin), return basic profile
//            return Ok(userDto);
//        }

//        [HttpGet("employee-profile"), Authorize(Roles = "Employee")]
//        public async Task<IActionResult> GetEmployeeProfile()
//        {
//            var userEmail = User.FindFirstValue(ClaimTypes.Email);
//            if (string.IsNullOrEmpty(userEmail))
//            {
//                return Unauthorized();
//            }

//            var user = await _authService.GetUserByEmailAsync(userEmail);
//            if (user == null)
//            {
//                return NotFound("User not found.");
//            }

//            if (user.Role != Enums.Employee)
//            {
//                return Forbid("Access denied. This endpoint is only for employees.");
//            }

//            var employeeProfile = new
//            {
//                id = user.UserId,
//                employeeId = user.UserId, // Employee ID is the same as User ID
//                userId = user.UserId,
//                email = user.Email,
//                firstName = user.FirstName,
//                lastName = user.LastName,
//                role = user.Role.ToString()
//            };

//            return Ok(employeeProfile);
//        }

//        [HttpGet("admin-data"), Authorize(Roles = "Admin")]
//        public IActionResult GetAdminData()
//        {
//            return Ok("This is sensitive data only for Admins.");
//        }

//        [HttpGet("employee-data"), Authorize(Roles = "Admin,Employee")]
//        public IActionResult GetEmployeeData()
//        {
//            return Ok("This data is for Admins and Employees.");
//        }
//    }
//}


// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.SignalR; // ← Add this
// using automobile_backend.Hubs;     // ← Add this (adjust namespace if needed)

// namespace automobile_backend.Controllers
// {
//     using System.Security.Claims;
//     using automobile_backend.InterFaces.IServices;
//     using automobile_backend.Models.DTOs;
//     using automobile_backend.Models.Entities;
//     using Microsoft.AspNetCore.Authentication;
//     using Microsoft.AspNetCore.Authentication.Google;
//     using Microsoft.AspNetCore.Authorization;
//     using Microsoft.AspNetCore.Mvc;

//     [Route("api/[controller]")]
//     [ApiController]
//     public class AuthController : ControllerBase
//     {
//         private readonly IAuthService _authService;
//         private readonly IHubContext<AdminNotificationHub> _hubContext; // ← Add this

//         public AuthController(IAuthService authService, IHubContext<AdminNotificationHub> hubContext)
//         {
//             _authService = authService;
//             _hubContext = hubContext; // ← Inject hub
//         }

//         [HttpPost("register/v1")]
//         public async Task<IActionResult> Register(UserRegisterDto request)
//         {
//             var user = await _authService.RegisterAsync(request);
//             if (user == null)
//             {
//                 return BadRequest("User with this email already exists.");
//             }

//             // === SEND REAL-TIME NOTIFICATION TO ADMINS ===
//             var newUserDto = new
//             {
//                 userId = user.UserId,
//                 fullName = $"{user.FirstName} {user.LastName}".Trim(),
//                 email = user.Email,
//                 role = user.Role.ToString(),
//                 registeredDate = DateTime.UtcNow.ToString("o") // ISO 8601
//             };

//             await _hubContext.Clients
//                 .Group(AdminNotificationHub.AdminGroup)
//                 .SendAsync("NewUserRegistered", newUserDto);

//             return Ok(new { message = "Registration successful" });
//         }

//         // === ALL OTHER METHODS UNCHANGED ===
//         [HttpPost("login/v1")]
//         public async Task<IActionResult> Login(UserLoginDto request)
//         {
//             var (user, token) = await _authService.LoginAsync(request);
//             if (user == null || token == null)
//             {
//                 return Unauthorized("Invalid credentials.");
//             }
//             Response.Cookies.Append("jwt-token", token, new CookieOptions
//             {
//                 HttpOnly = true,
//                 Secure = false,
//                 SameSite = SameSiteMode.Strict,
//                 Expires = DateTime.UtcNow.AddDays(1)
//             });
//             var userDto = new
//             {
//                 email = user.Email,
//                 firstName = user.FirstName,
//                 lastName = user.LastName,
//                 role = user.Role.ToString()
//             };
//             return Ok(userDto);
//         }

//         [HttpGet("google-login")]
//         public IActionResult GoogleLogin()
//         {
//             var redirectUrl = Url.Action(nameof(GoogleSignInHandler));
//             var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
//             return Challenge(properties, GoogleDefaults.AuthenticationScheme);
//         }

//         [HttpGet("google-signin-handler")]
//         public async Task<IActionResult> GoogleSignInHandler()
//         {
//             try
//             {
//                 var (user, jwtToken) = await _authService.HandleGoogleLoginAsync();
//                 Response.Cookies.Append("jwt-token", jwtToken, new CookieOptions
//                 {
//                     HttpOnly = true,
//                     Secure = false,
//                     SameSite = SameSiteMode.Strict,
//                     Expires = DateTime.UtcNow.AddDays(1)
//                 });
//                 string redirectUrl = user.Role switch
//                 {
//                     Enums.Admin => "http://localhost:3000/admin/dashboard",
//                     Enums.Employee => "http://localhost:3000/employee/dashboard",
//                     _ => "http://localhost:3000/customer/dashboard"
//                 };
//                 return Redirect(redirectUrl);
//             }
//             catch (Exception ex)
//             {
//                 return Redirect($"http://localhost:3000/login?error={Uri.EscapeDataString(ex.Message)}");
//             }
//         }

//         [HttpPost("logout"), Authorize]
//         public IActionResult Logout()
//         {
//             Response.Cookies.Delete("jwt-token");
//             return Ok(new { message = "Logout successful" });
//         }

//         [HttpGet("profile"), Authorize]
//         public async Task<IActionResult> GetProfile()
//         {
//             var userEmail = User.FindFirstValue(ClaimTypes.Email);
//             if (string.IsNullOrEmpty(userEmail))
//             {
//                 return Unauthorized();
//             }
//             var user = await _authService.GetUserByEmailAsync(userEmail);
//             if (user == null)
//             {
//                 return NotFound("User not found.");
//             }
//             var userDto = new
//             {
//                 id = user.UserId,
//                 email = user.Email,
//                 firstName = user.FirstName,
//                 lastName = user.LastName,
//                 role = user.Role.ToString()
//             };
//             if (user.Role == Enums.Employee)
//             {
//                 var employeeProfile = new
//                 {
//                     userDto.id,
//                     employeeId = user.UserId,
//                     userDto.email,
//                     userDto.firstName,
//                     userDto.lastName,
//                     userDto.role
//                 };
//                 return Ok(employeeProfile);
//             }
//             else if (user.Role == Enums.Customer)
//             {
//                 var customerProfile = new
//                 {
//                     userDto.id,
//                     customerId = user.UserId,
//                     userDto.email,
//                     userDto.firstName,
//                     userDto.lastName,
//                     userDto.role
//                 };
//                 return Ok(customerProfile);
//             }
//             return Ok(userDto);
//         }

//         [HttpGet("employee-profile"), Authorize(Roles = "Employee")]
//         public async Task<IActionResult> GetEmployeeProfile()
//         {
//             var userEmail = User.FindFirstValue(ClaimTypes.Email);
//             if (string.IsNullOrEmpty(userEmail))
//             {
//                 return Unauthorized();
//             }
//             var user = await _authService.GetUserByEmailAsync(userEmail);
//             if (user == null)
//             {
//                 return NotFound("User not found.");
//             }
//             if (user.Role != Enums.Employee)
//             {
//                 return Forbid("Access denied. This endpoint is only for employees.");
//             }
//             var employeeProfile = new
//             {
//                 id = user.UserId,
//                 employeeId = user.UserId,
//                 userId = user.UserId,
//                 email = user.Email,
//                 firstName = user.FirstName,
//                 lastName = user.LastName,
//                 role = user.Role.ToString()
//             };
//             return Ok(employeeProfile);
//         }

//         [HttpGet("admin-data"), Authorize(Roles = "Admin")]
//         public IActionResult GetAdminData()
//         {
//             return Ok("This is sensitive data only for Admins.");
//         }

//         [HttpGet("employee-data"), Authorize(Roles = "Admin,Employee")]
//         public IActionResult GetEmployeeData()
//         {
//             return Ok("This data is for Admins and Employees.");
//         }
//     }
// }

using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;

namespace automobile_backend.Controllers
{
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
            var (user, token) = await _authService.LoginAsync(request);
            if (user == null || token == null)
            {
                return Unauthorized("Invalid credentials.");
            }

            Response.Cookies.Append("jwt-token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(1)
            });

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
                    Secure = false,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(1)
                });

                string redirectUrl = user.Role switch
                {
                    Enums.Admin => "http://localhost:3000/admin/dashboard",
                    Enums.Employee => "http://localhost:3000/employee/dashboard",
                    _ => "http://localhost:3000/customer/dashboard"
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
            Response.Cookies.Delete("jwt-token");
            return Ok(new { message = "Logout successful" });
        }

        [HttpGet("profile"), Authorize]
        public async Task<IActionResult> GetProfile()
        {
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
                id = user.UserId,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                role = user.Role.ToString()
            };

            if (user.Role == Enums.Employee)
            {
                return Ok(new
                {
                    userDto.id,
                    employeeId = user.UserId,
                    userDto.email,
                    userDto.firstName,
                    userDto.lastName,
                    userDto.role
                });
            }
            else if (user.Role == Enums.Customer)
            {
                return Ok(new
                {
                    userDto.id,
                    customerId = user.UserId,
                    userDto.email,
                    userDto.firstName,
                    userDto.lastName,
                    userDto.role
                });
            }

            return Ok(userDto);
        }

        [HttpGet("employee-profile"), Authorize(Roles = "Employee")]
        public async Task<IActionResult> GetEmployeeProfile()
        {
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

            if (user.Role != Enums.Employee)
            {
                return Forbid("Access denied. This endpoint is only for employees.");
            }

            return Ok(new
            {
                id = user.UserId,
                employeeId = user.UserId,
                userId = user.UserId,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                role = user.Role.ToString()
            });
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
