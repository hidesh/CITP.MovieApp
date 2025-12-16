using System.ComponentModel.DataAnnotations;

namespace CITP.MovieApp.Application.DTOs
{
    // ---------------- LOGIN ----------------
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }

    // ---------------- REGISTER ----------------
    public class RegisterRequest
    {
        // LOGIN IDENTIFIER (immutable)
        [Required]
        [MinLength(3)]
        [MaxLength(30)]
        public string Username { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = null!;

        // UI DISPLAY NAME (mutable)
        [MaxLength(100)]
        public string? FullName { get; set; }
    }

    // ---------------- UPDATE PROFILE ----------------
    public class UpdateProfileRequest
    {
        [MaxLength(100)]
        public string? FullName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
    }

    // ---------------- UPDATE PASSWORD ----------------
    public class UpdatePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; } = null!;

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; } = null!;
    }
}