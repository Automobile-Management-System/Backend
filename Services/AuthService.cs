using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using automobile_backend.InterFaces.IRepository;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.DTOs;
using automobile_backend.Models.Entities;
using Microsoft.AspNetCore.Authentication; // Added for auth methods
using Microsoft.IdentityModel.Tokens;

namespace automobile_backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // This MUST match the string used in Program.cs
        private const string ExternalCookieAuthenticationScheme = "ExternalCookie";

        public AuthService(IAuthRepository authRepository, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _authRepository = authRepository;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<User?> RegisterAsync(UserRegisterDto request)
        {
            if (await _authRepository.GetUserByEmailAsync(request.Email) != null)
            {
                return null; // User already exists
            }

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Role = Enums.Customer // Default role
            };

            return await _authRepository.CreateUserAsync(user);
        }

        public async Task<string?> LoginAsync(UserLoginDto request)
        {
            var user = await _authRepository.GetUserByEmailAsync(request.Email);

            // Ensure user exists and has a password (not a Google-only user)
            if (user == null || user.PasswordHash == null || user.PasswordSalt == null)
            {
                return null;
            }

            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return null; // Invalid credentials
            }

            return CreateJwtToken(user);
        }

        // --- Google Auth Method ---

        public async Task<(User user, string jwtToken)> HandleGoogleLoginAsync()
        {
            // FIX 2: Use the private field _httpContextAccessor (with underscore)
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new InvalidOperationException("HTTP context is not available.");
            }

            // 1. Get claims from the temporary cookie
            var authResult = await httpContext.AuthenticateAsync(ExternalCookieAuthenticationScheme);
            if (!authResult.Succeeded || authResult.Principal == null)
            {
                throw new Exception("External authentication failed.");
            }

            var claims = authResult.Principal.Claims;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (email == null)
            {
                throw new Exception("Email claim not found from Google.");
            }

            // 2. Find or create the local user
            var user = await _authRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                // User doesn't exist, create a new one
                user = new User
                {
                    Email = email,
                    FirstName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? string.Empty,
                    LastName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value ?? string.Empty,
                    PasswordHash = null, // No password for Google users
                    PasswordSalt = null,
                    Role = Enums.Customer,
                    Status = "Active"
                };
                user = await _authRepository.CreateUserAsync(user);
            }

            // 3. Clean up the temporary external cookie
            await httpContext.SignOutAsync(ExternalCookieAuthenticationScheme);

            // 4. Create and return our application's JWT
            var jwtToken = CreateJwtToken(user);
            return (user, jwtToken);
        }

        // --- Private Helper Methods ---

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private string CreateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = creds,
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}