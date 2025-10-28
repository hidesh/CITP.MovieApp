using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CITP.MovieApp.Domain.Entities

{
    [Table("searchhistory")]
    public class SearchHistory
    {
        [Key]
        [Column("search_id")]
        public int SearchId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("tconst")]
        public string? Tconst { get; set; } // Nullable if search is not tied to a title

        [Column("visited_at")]
        public DateTime VisitedAt { get; set; }

        // Navigation properties
        public User? User { get; set; }
        public Title? Title { get; set; }
    }
}