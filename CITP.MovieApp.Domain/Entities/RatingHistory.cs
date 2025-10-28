using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CITP.MovieApp.Domain.Entities

{
    [Table("ratinghistory")]
    public class RatingHistory
    {
        [Key]
        [Column("rating_id")]
        public int RatingId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("tconst")]
        public string Tconst { get; set; } = null!; // References Title

        [Column("rating")]
        public decimal Rating { get; set; }

        [Column("rated_at")]
        public DateTime RatedAt { get; set; }

        // Navigation properties
        public User? User { get; set; }
        public Title? Title { get; set; }
    }
}
