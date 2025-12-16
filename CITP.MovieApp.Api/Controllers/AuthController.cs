using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using CITP.MovieApp.Application.DTOs;
using CITP.MovieApp.Domain.Entities;
using CITP.MovieApp.Infrastructure.Repositories;

namespace CITP.MovieApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserRepository _users;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        private const string DEFAULT_AVATAR = "/uploads/avatars/default.png";

        // Allowed image extensions ONLY
        private static readonly HashSet<string> AllowedImageExtensions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".webp"
            };

        // Explicitly blocked (defense in depth)
        private static readonly HashSet<string> BlockedExtensions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ".exe", ".bat", ".cmd", ".sh", ".ps1"
            };

        public AuthController(
            UserRepository users,
            IConfiguration config,
            IWebHostEnvironment env)
        {
            _users = users;
            _config = config;
            _env = env;
        }

        // ---------------- REGISTER ----------------
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(
            [FromForm] RegisterRequest req,
            IFormFile? avatar)
        {
            if (await _users.GetByUsernameAsync(req.Username) != null)
                return BadRequest(new { message = "Username already exists" });

            if (await _users.GetByEmailAsync(req.Email) != null)
                return BadRequest(new { message = "Email already exists" });

            var avatarUrl = DEFAULT_AVATAR;

            if (avatar != null)
            {
                var result = await TrySaveAvatarAsync(avatar);
                if (!result.Success)
                    return BadRequest(new { message = result.Error });

                avatarUrl = result.Path!;
            }

            var user = new User
            {
                Username = req.Username,
                Email = req.Email,
                PasswordHash = HashPassword(req.Password),
                FullName = req.FullName,
                ProfileImageUrl = avatarUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _users.AddAsync(user);
            await _users.SaveAsync();

            return Ok(new { message = "User registered successfully" });
        }

        // ---------------- LOGIN ----------------
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequest req)
        {
            var user = await _users.GetByUsernameAsync(req.Username);

            if (user == null || !VerifyPassword(req.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid username or password" });

            return Ok(new
            {
                user = new
                {
                    user.UserId,
                    user.Username,
                    user.Email,
                    user.FullName,
                    user.ProfileImageUrl
                },
                token = GenerateJwtToken(user)
            });
        }

        // ---------------- ME ----------------
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            return Ok(new
            {
                user.UserId,
                user.Username,
                user.Email,
                user.FullName,
                user.ProfileImageUrl
            });
        }

        // ---------------- UPDATE PROFILE ----------------
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(
            [FromForm] UpdateProfileRequest req,
            IFormFile? avatar)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            if (!string.IsNullOrWhiteSpace(req.FullName))
                user.FullName = req.FullName;

            if (!string.IsNullOrWhiteSpace(req.Email) &&
                !string.Equals(req.Email, user.Email, StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _users.GetByEmailAsync(req.Email);
                if (existing != null && existing.UserId != user.UserId)
                    return BadRequest(new { message = "Email already exists" });

                user.Email = req.Email;
            }

            if (avatar != null)
            {
                var result = await TrySaveAvatarAsync(avatar);
                if (!result.Success)
                    return BadRequest(new { message = result.Error });

                user.ProfileImageUrl = result.Path!;
            }

            await _users.SaveAsync();

            return Ok(new
            {
                message = "Profile updated",
                user = new
                {
                    user.UserId,
                    user.Username,
                    user.Email,
                    user.FullName,
                    user.ProfileImageUrl
                }
            });
        }

        // ---------------- UPDATE PASSWORD ----------------
        [HttpPut("password")]
        [Authorize]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordRequest req)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            if (!VerifyPassword(req.CurrentPassword, user.PasswordHash))
                return BadRequest(new { message = "Current password is incorrect" });

            if (VerifyPassword(req.NewPassword, user.PasswordHash))
                return BadRequest(new { message = "New password must be different" });

            user.PasswordHash = HashPassword(req.NewPassword);
            await _users.SaveAsync();

            return Ok(new { message = "Password updated successfully" });
        }

        // ---------------- HELPERS ----------------

        private async Task<User?> GetCurrentUserAsync()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return idClaim != null && int.TryParse(idClaim.Value, out var id)
                ? await _users.GetByIdAsync(id)
                : null;
        }

        private async Task<(bool Success, string? Path, string? Error)> TrySaveAvatarAsync(IFormFile file)
        {
            if (file.Length == 0)
                return (false, null, "Uploaded file is empty");

            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(extension) ||
                BlockedExtensions.Contains(extension) ||
                !AllowedImageExtensions.Contains(extension))
            {
                return (false, null, "Only JPG, PNG or WEBP images are allowed");
            }

            // Ensure wwwroot exists
            var webRoot = _env.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot))
            {
                webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");
                Directory.CreateDirectory(webRoot);
            }

            var uploadDir = Path.Combine(webRoot, "uploads", "avatars");
            Directory.CreateDirectory(uploadDir);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadDir, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return (true, $"/uploads/avatars/{fileName}", null);
        }

        private string GenerateJwtToken(User user)
        {
            var jwt = _config.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwt["Key"]!);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            return new JwtSecurityTokenHandler().WriteToken(
                new JwtSecurityToken(
                    issuer: jwt["Issuer"],
                    audience: jwt["Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpiryMinutes"]!)),
                    signingCredentials: new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256
                    )
                )
            );
        }

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        private static bool VerifyPassword(string password, string hash)
            => HashPassword(password) == hash;
    }
}
