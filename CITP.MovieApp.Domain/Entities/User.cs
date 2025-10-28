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

        [Column("username")]
        [Required]
        public string Username { get; set; } = null!;

        [Column("email")]
        [Required]
        public string Email { get; set; } = null!;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("password")]
        [Required]
        public string PasswordHash { get; set; } = null!; // Store hashed password
        

        // Navigation properties
        public ICollection<RatingHistory>? RatingHistories { get; set; }
        public ICollection<SearchHistory>? SearchHistories { get; set; }
        public ICollection<Bookmark>? Bookmarks { get; set; }
        public ICollection<Note>? Notes { get; set; }
    }
}