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
            if (!IsValidEmail(req.Email))
                return BadRequest(new { message = "Invalid email format" });

            if (await _users.GetByUsernameAsync(req.Username) != null)
                return BadRequest(new { message = "Username already exists" });

            if (await _users.GetByEmailAsync(req.Email) != null)
                return BadRequest(new { message = "Email already exists" });

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

            if (user == null || !VerifyPassword(req.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid Username and/or password" });

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                user = new
                {
                    userId = user.UserId,
                    username = user.Username,
                    email = user.Email
                },
                token
            });
        }

        // ✅ REQUIRED BY FRONTEND
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim == null) return Unauthorized();

            if (!int.TryParse(idClaim.Value, out var userId))
                return Unauthorized();

            var user = await _users.GetByIdAsync(userId);
            if (user == null) return Unauthorized();

            return Ok(new
            {
                userId = user.UserId,
                username = user.Username,
                email = user.Email
            });
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSection = _config.GetSection("Jwt");
            var keyBytes = Encoding.ASCII.GetBytes(jwtSection.GetValue<string>("Key")!);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            int expiryMinutes = int.TryParse(jwtSection["ExpiryMinutes"], out var val) ? val : 60;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
                SigningCredentials = creds,
                Issuer = jwtSection["Issuer"],
                Audience = jwtSection["Audience"]
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            return HashPassword(password) == storedHash;
        }

        private static bool IsValidEmail(string email)
        {
            return MyRegex().IsMatch(email);
        }

        [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
        private static partial Regex MyRegex();
    }
}
