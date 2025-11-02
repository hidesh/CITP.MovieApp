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
using Microsoft.AspNetCore.Authorization;

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
        [AllowAnonymous]
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
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequest req)
        {
            var user = await _users.GetByUsernameAsync(req.Username);

            // Unified error message
            if (user == null || !VerifyPassword(req.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid Username and/or password" });

            var token = GenerateJwtToken(user);
            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        // Token generation aligned with Program.cs
        private string GenerateJwtToken(User user)
        {
            var jwtSection = _config.GetSection("Jwt");

            // Ensure the same encoding & key format as Program.cs
            var keyBytes = Encoding.ASCII.GetBytes(jwtSection["Key"] ?? throw new Exception("JWT key missing"));
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            // Use safe int parse with fallback
            int expiryMinutes = int.TryParse(jwtSection["ExpiryMinutes"], out var val) ? val : 60;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
                SigningCredentials = creds,
                Issuer = jwtSection["Issuer"],
                Audience = jwtSection["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
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
