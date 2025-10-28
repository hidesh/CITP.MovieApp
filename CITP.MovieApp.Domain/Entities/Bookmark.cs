using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CITP.MovieApp.Domain.Entities
{
    [Table("bookmark")]
    public class Bookmark
    {
        [Key]
        [Column("bookmark_id")]
        public int BookmarkId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("tconst")]
        public string? Tconst { get; set; } // Nullable if bookmark can be for a title

        [Column("nconst")]
        public string? Nconst { get; set; } // Nullable if bookmark can be for a person

        [Column("bookmarked_at")]
        public DateTime BookmarkedAt { get; set; }

        // Navigation properties
        public User? User { get; set; }
        public Title? Title { get; set; }
        public Person? Person { get; set; }
    }
}