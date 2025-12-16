using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CITP.MovieApp.Domain.Entities
{
    [Table("User")]
    public class User
    {
        [Key]
        [Column("user_id")]
        public int UserId { get; set; }

        // LOGIN IDENTIFIER (immutable)
        [Column("username")]
        [Required]
        public string Username { get; set; } = null!;

        // CONTACT / LOGIN
        [Column("email")]
        [Required]
        public string Email { get; set; } = null!;

        [Column("password")]
        [Required]
        public string PasswordHash { get; set; } = null!;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // UI DISPLAY NAME (mutable)
        [Column("full_name")]
        public string? FullName { get; set; }

        // LOCAL IMAGE URL (dev)
        [Column("profile_image_url")]
        public string? ProfileImageUrl { get; set; }

        // Navigation
        public ICollection<RatingHistory>? RatingHistories { get; set; }
        public ICollection<SearchHistory>? SearchHistories { get; set; }
        public ICollection<Bookmark>? Bookmarks { get; set; }
        public ICollection<Note>? Notes { get; set; }
    }
}