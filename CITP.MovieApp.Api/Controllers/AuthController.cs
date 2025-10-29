using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using CITP.MovieApp.Application.DTOs;
using CITP.MovieApp.Domain.Entities;
using CITP.MovieApp.Infrastructure.Repositories;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace CITP.MovieApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public partial class AuthController : ControllerBase
    {
        private readonly UserRepository _users;
        private readonly IConfiguration _config;

        public AuthController(UserRepository users, IConfiguration config)
        {
            _users = users;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest req)
        {
            // Validate email format
            if (!IsValidEmail(req.Email))
                return BadRequest(new { message = "Invalid email format" });

            // Check if username or email already exists
            if (await _users.GetByUsernameAsync(req.Username) != null)
                return BadRequest(new { message = "Username already exists" });

            if (await _users.GetByEmailAsync(req.Email) != null)
                return BadRequest(new { message = "Email already exists" });

            // Hash password securely
            var hashedPassword = HashPassword(req.Password);

            var user = new User
            {
                Username = req.Username,
                Email = req.Email,
                PasswordHash = hashedPassword,
                CreatedAt = DateTime.UtcNow
            };

            await _users.AddAsync(user);
            await _users.SaveAsync();

            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest req)
        {
            var user = await _users.GetByUsernameAsync(req.Username);

            // Unified error message
            if (user == null || !VerifyPassword(req.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid Username and/or password" });
            }

            var token = GenerateJwtToken(user.Username);
            return Ok(new { token });
        }

        private string GenerateJwtToken(string username)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSection.GetValue<string>("Key")!);

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username)
                }),
                Expires = DateTime.UtcNow.AddMinutes(jwtSection.GetValue<int>("ExpiryMinutes")),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSection.GetValue<string>("Issuer"),
                Audience = jwtSection.GetValue<string>("Audience")
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // SHA256 hashing 
        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == storedHash;
        }

        // Basic email validation
        private static bool IsValidEmail(string email)
        {
            var regex = MyRegex();
            return regex.IsMatch(email);
        }

        [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
        private static partial Regex MyRegex();
    }
}
