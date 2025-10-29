using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CITP.MovieApp.Domain.Entities

{
    [Table("rating")]
    public class Rating
    {
        [Key]
        [Column("tconst")]
        public string Tconst { get; set; } = null!; // References Title

        [Column("averagerating")]
        public decimal AverageRating { get; set; }

        [Column("numvotes")]
        public int NumVotes { get; set; }

        // Navigation property
        [ForeignKey("Tconst")]
        public Title? Title { get; set; }
    }
}
